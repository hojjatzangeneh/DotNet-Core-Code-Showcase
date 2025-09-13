using Memory_Cache.Models;
using Memory_Cache.Repositories;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Memory_Cache.Services;

public class DistributedCacheProductService : IProductService
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new();

    readonly IDistributedCache _cache;
    readonly DistributedCacheEntryOptions _distOpts;
    readonly ILogger<DistributedCacheProductService> _logger;
    readonly IProductRepository _repo;
    readonly DistributedCacheSettings _settings;

    public DistributedCacheProductService(
        IDistributedCache distributed,
        IProductRepository repo,
        ILogger<DistributedCacheProductService> logger,
        IOptions<DistributedCacheSettings> options)
    {
        _cache = distributed;
        _repo = repo;
        _logger = logger;
        _settings = options.Value ?? new DistributedCacheSettings();

        _distOpts = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_settings.ProductTtlSeconds)
        };
    }

    string BuildKey(int id)
    {
        return $"{_settings.KeyPrefix}{id}";
    }

    static T? Deserialize <T>(byte[] bytes)
    {
        try
        {
            // try gzip header (1F 8B)
            bool isGzip = (bytes.Length >= 2) && (bytes[0] == 0x1F) && (bytes[1] == 0x8B);
            if(isGzip)
            {
                using MemoryStream src = new(bytes);
                using GZipStream gz = new(src, CompressionMode.Decompress);
                using MemoryStream dst = new();
                gz.CopyTo(dst);
                return JsonSerializer.Deserialize<T>(dst.ToArray(), Json);
            }

            return JsonSerializer.Deserialize<T>(bytes, Json);
        }
        catch
        {
            return default;
        }
    }

    static SemaphoreSlim Gate(string key)
    {
        return Locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
    }

    async Task RefreshInBackgroundAsync(int id, string key, CancellationToken ct)
    {
        try
        {
            SemaphoreSlim gate = Gate(key);
            if(!(await gate.WaitAsync(0, ct)))
            {
                return; // someone else refreshing locally
            }

            try
            {
                Product? product = await _repo.GetByIdAsync(id, ct);

                if(product is null)
                {
                    if(_settings.EnableNegativeCaching)
                    {
                        Envelope<Product> envNull = Envelope<Product>.Null(
                            DateTime.UtcNow.AddSeconds(_settings.NullTtlSeconds));
                        await SetAsync(
                            key,
                            envNull,
                            new DistributedCacheEntryOptions
                            {
                                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_settings.NullTtlSeconds)
                            },
                            ct);
                        _logger.LogInformation("[Distributed] REFRESH → NULL {Key}", key);
                    }
                    else
                    {
                        await _cache.RemoveAsync(key, ct);
                    }
                    return;
                }

                Envelope<Product> env = Envelope<Product>.From(
                    product,
                    DateTime.UtcNow.AddSeconds(_settings.ProductTtlSeconds));
                await SetAsync(key, env, _distOpts, ct);
                _logger.LogInformation("[Distributed] REFRESH → SET {Key}", key);
            }
            finally
            {
                gate.Release();
            }
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "[Distributed] REFRESH failed for {Key}", key);
        }
    }

    byte[] Serialize <T>(T value)
    {
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(value, Json);
        if(!_settings.EnableCompression)
        {
            return json;
        }

        using MemoryStream src = new(json);
        using MemoryStream dst = new();
        using(GZipStream gz = new(dst, CompressionLevel.Fastest, leaveOpen: true))
        {
            src.CopyTo(gz);
        }

        return dst.ToArray();
    }

    // ----------------- low-level helpers -----------------

    async Task SetAsync <T>(
        string key,
        Envelope<T> env,
        DistributedCacheEntryOptions opts,
        CancellationToken ct)
    {
        byte[] payload = Serialize(env);
        await _cache.SetAsync(key, payload, opts, ct);
    }

    public async Task<Product?> GetAsync(int id, CancellationToken ct = default)
    {
        string key = BuildKey(id);

        // 1) Try distributed cache
        byte[]? bytes = await _cache.GetAsync(key, ct);
        if(bytes is not null)
        {
            Envelope<Product>? env = Deserialize<Envelope<Product>>(bytes);
            if(env is not null)
            {
                // Soft-ttl early refresh
                TimeSpan remaining = env.ExpiresAtUtc - DateTime.UtcNow;
                if((_settings.EarlyRefreshSeconds > 0) &&
                    (remaining <= TimeSpan.FromSeconds(_settings.EarlyRefreshSeconds)) &&
                    env.TryBeginRefresh())
                {
                    _ = RefreshInBackgroundAsync(id, key, ct);
                }

                _logger.LogInformation("[Distributed] HIT {Key}", key);
                return env.IsNull ? null : env.Value;
            }
        }

        _logger.LogInformation("[Distributed] MISS {Key}", key);

        // 2) Prevent local stampede on this node
        SemaphoreSlim gate = Gate(key);
        await gate.WaitAsync(ct);
        try
        {
            // Double-check after acquiring the lock
            bytes = await _cache.GetAsync(key, ct);
            if(bytes is not null)
            {
                Envelope<Product>? env2 = Deserialize<Envelope<Product>>(bytes);
                if(env2 is not null)
                {
                    _logger.LogInformation("[Distributed] HIT2 {Key}", key);
                    return env2.IsNull ? null : env2.Value;
                }
            }

            // 3) Load from source of truth
            Product? product = await _repo.GetByIdAsync(id, ct);

            if(product is null)
            {
                if(_settings.EnableNegativeCaching)
                {
                    Envelope<Product> nullEnv = Envelope<Product>.Null(
                        DateTime.UtcNow.AddSeconds(_settings.NullTtlSeconds));
                    await SetAsync(
                        key,
                        nullEnv,
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_settings.NullTtlSeconds)
                        },
                        ct);
                    _logger.LogInformation("[Distributed] SET NULL {Key}", key);
                }
                return null;
            }

            // 4) Normal cache set (with soft expiry inside the envelope)
            Envelope<Product> env = Envelope<Product>.From(
                product,
                DateTime.UtcNow.AddSeconds(_settings.ProductTtlSeconds));
            await SetAsync(key, env, _distOpts, ct);
            _logger.LogInformation("[Distributed] SET {Key}", key);

            return product;
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task UpdatePriceAsync(int id, decimal price, CancellationToken ct = default)
    {
        await _repo.UpdatePriceAsync(id, price, ct);

        string key = BuildKey(id);

        // Invalidate
        await _cache.RemoveAsync(key, ct);
        _logger.LogInformation("[Distributed] INVALIDATE {Key}", key);

        // Optional write-through: immediately set fresh value to reduce next-latency
        try
        {
            Product? product = await _repo.GetByIdAsync(id, ct);
            if(product is null)
            {
                if(_settings.EnableNegativeCaching)
                {
                    Envelope<Product> envNull = Envelope<Product>.Null(
                        DateTime.UtcNow.AddSeconds(_settings.NullTtlSeconds));
                    await SetAsync(
                        key,
                        envNull,
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_settings.NullTtlSeconds)
                        },
                        ct);
                    _logger.LogInformation("[Distributed] SET NULL (WriteThrough) {Key}", key);
                }
                return;
            }

            Envelope<Product> env = Envelope<Product>.From(
                product,
                DateTime.UtcNow.AddSeconds(_settings.ProductTtlSeconds));
            await SetAsync(key, env, _distOpts, ct);
            _logger.LogInformation("[Distributed] SET (WriteThrough) {Key}", key);
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "[Distributed] WriteThrough failed for {Key}", key);
        }
    }

    // Envelope keeps value + soft-expiry + a tiny refresh flag
}