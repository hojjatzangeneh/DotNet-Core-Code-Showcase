using Memory_Cache.Models;
using Memory_Cache.Repositories;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text.Json;

namespace Memory_Cache.Services;

public class HybridCacheProductService : IProductService
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new();
    readonly IDistributedCache _dist;

    readonly DistributedCacheEntryOptions _distOpts;
    readonly ILogger<HybridCacheProductService> _logger;

    readonly IMemoryCache _memory;
    readonly IProductRepository _repo;
    readonly HybridCacheSettings _settings;

    public HybridCacheProductService(
        IMemoryCache memory,
        IDistributedCache distributed,
        IProductRepository repo,
        ILogger<HybridCacheProductService> logger,
        IOptions<HybridCacheSettings> options)
    {
        _memory = memory;
        _dist = distributed;
        _repo = repo;
        _logger = logger;
        _settings = options.Value ?? new HybridCacheSettings();

        _distOpts = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_settings.DistributedTtlSeconds)
        };
    }

    static T? Deserialize <T>(byte[] bytes)
    {
        try
        {
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

    string DistKey(int id)
    {
        return $"{_settings.DistKeyPrefix}{id}";
    }

    static SemaphoreSlim Gate(string key)
    {
        return Locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
    }

    string MemKey(int id)
    {
        return $"{_settings.MemoryKeyPrefix}{id}";
    }

    async Task RefreshInBackgroundAsync(int id, CancellationToken ct)
    {
        string memKey = MemKey(id);
        string distKey = DistKey(id);

        try
        {
            SemaphoreSlim gate = Gate(distKey);
            if(!(await gate.WaitAsync(0, ct)))
            {
                return; // someone else refreshing locally
            }

            try
            {
                Product? p = await _repo.GetByIdAsync(id, ct);
                if(p is null)
                {
                    if(_settings.EnableNegativeCaching)
                    {
                        Envelope<Product> envNull = Envelope<Product>.Null(
                            DateTime.UtcNow.AddSeconds(_settings.NullTtlSeconds));
                        await SetDistributedAsync(
                            distKey,
                            envNull,
                            new DistributedCacheEntryOptions
                            {
                                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_settings.NullTtlSeconds)
                            },
                            ct);
                        SetMemory(memKey, null, ttlSeconds: _settings.NullTtlSeconds);
                        _logger.LogInformation("[Hybrid] REFRESH → NULL {Key}", distKey);
                    }
                    else
                    {
                        await _dist.RemoveAsync(distKey, ct);
                        _memory.Remove(memKey);
                    }
                    return;
                }

                Envelope<Product> envOk = Envelope<Product>.From(
                    p,
                    DateTime.UtcNow.AddSeconds(_settings.DistributedTtlSeconds));
                await SetDistributedAsync(distKey, envOk, _distOpts, ct);
                SetMemory(memKey, p);
                _logger.LogInformation("[Hybrid] REFRESH → SET {Key}", distKey);
            }
            finally
            {
                gate.Release();
            }
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "[Hybrid] REFRESH failed for {Id}", id);
        }
        finally
        {
            // clear Refreshing flag if still in memory
            if(_memory.TryGetValue(memKey, out CacheEnvelope? env))
            {
                env?.EndRefresh();
            }
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

    // ----------------- Distributed helpers -----------------
    async Task SetDistributedAsync <T>(
        string key,
        Envelope<T> env,
        DistributedCacheEntryOptions opts,
        CancellationToken ct)
    {
        byte[] payload = Serialize(env);
        await _dist.SetAsync(key, payload, opts, ct);
    }

    // ----------------- Memory helpers -----------------
    void SetMemory(string memKey, Product? value, int? ttlSeconds = null)
    {
        TimeSpan ttl = TimeSpan.FromSeconds(ttlSeconds ?? _settings.MemoryTtlSeconds);
        CacheEnvelope env = (value is null)
            ? CacheEnvelope.Null(DateTimeOffset.UtcNow.Add(ttl))
            : (new CacheEnvelope(value, DateTimeOffset.UtcNow.Add(ttl)));

        MemoryCacheEntryOptions opts = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(ttl)
            .SetPriority(CacheItemPriority.Normal)
            .SetSize(_settings.MemoryEntrySize)
            .RegisterPostEvictionCallback(
                (k, _, reason, __) =>
                _logger.LogInformation("[Hybrid] Memory EVICT {Key} reason {Reason}", k, reason));

        _memory.Set(memKey, env, opts);
    }

    public async Task<Product?> GetAsync(int id, CancellationToken ct = default)
    {
        string memKey = MemKey(id);

        // 1) Try Memory
        if(_memory.TryGetValue(memKey, out CacheEnvelope envMem))
        {
            _logger.LogInformation("[Hybrid] Memory HIT {Key}", memKey);
            if(envMem.ValueIsNull)
            {
                return null;
            }

            // Early refresh window
            TimeSpan remaining = envMem.ExpiresAt - DateTimeOffset.UtcNow;
            if((_settings.EarlyRefreshSeconds > 0) &&
                (remaining <= TimeSpan.FromSeconds(_settings.EarlyRefreshSeconds)) &&
                envMem.TryBeginRefresh())
            {
                _ = RefreshInBackgroundAsync(id, ct);
            }

            return (Product) envMem.Value!;
        }

        // 2) Try Distributed
        string distKey = DistKey(id);
        byte[]? bytes = await _dist.GetAsync(distKey, ct);
        if(bytes is not null)
        {
            Envelope<Product>? envDist = Deserialize<Envelope<Product>>(bytes);
            if(envDist is not null)
            {
                _logger.LogInformation("[Hybrid] Distributed HIT {Key}", distKey);

                // warm memory
                SetMemory(memKey, envDist.IsNull ? null : envDist.Value);

                // trigger early refresh if near expiry
                TimeSpan remaining = envDist.ExpiresAtUtc - DateTime.UtcNow;
                if((_settings.EarlyRefreshSeconds > 0) &&
                    (remaining <= TimeSpan.FromSeconds(_settings.EarlyRefreshSeconds)) &&
                    envDist.TryBeginRefresh())
                {
                    _ = RefreshInBackgroundAsync(id, ct);
                }

                return envDist.IsNull ? null : envDist.Value;
            }
        }

        _logger.LogInformation("[Hybrid] MISS → loading from repository…");

        // 3) Prevent stampede (per-node)
        SemaphoreSlim gate = Gate(distKey);
        await gate.WaitAsync(ct);
        try
        {
            // Double-check after lock
            if(_memory.TryGetValue(memKey, out envMem))
            {
                _logger.LogInformation("[Hybrid] Memory HIT2 {Key}", memKey);
                return envMem.ValueIsNull ? null : ((Product) envMem.Value!);
            }

            bytes = await _dist.GetAsync(distKey, ct);
            if(bytes is not null)
            {
                Envelope<Product>? envDist2 = Deserialize<Envelope<Product>>(bytes);
                if(envDist2 is not null)
                {
                    _logger.LogInformation("[Hybrid] Distributed HIT2 {Key}", distKey);
                    SetMemory(memKey, envDist2.IsNull ? null : envDist2.Value);
                    return envDist2.IsNull ? null : envDist2.Value;
                }
            }

            // 4) Load from source of truth
            Product? product = await _repo.GetByIdAsync(id, ct);

            if(product is null)
            {
                if(_settings.EnableNegativeCaching)
                {
                    // dist
                    Envelope<Product> nullEnv = Envelope<Product>.Null(
                        DateTime.UtcNow.AddSeconds(_settings.NullTtlSeconds));
                    await SetDistributedAsync(
                        distKey,
                        nullEnv,
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_settings.NullTtlSeconds)
                        },
                        ct);
                    // mem
                    SetMemory(memKey, null, ttlSeconds: _settings.NullTtlSeconds);

                    _logger.LogInformation("[Hybrid] SET NULL for {Key}", distKey);
                }
                return null;
            }

            // 5) Set both layers (normal)
            Envelope<Product> envOk = Envelope<Product>.From(
                product,
                DateTime.UtcNow.AddSeconds(_settings.DistributedTtlSeconds));
            await SetDistributedAsync(distKey, envOk, _distOpts, ct);
            SetMemory(memKey, product);

            _logger.LogInformation("[Hybrid] SET both layers for {Id}", id);
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

        // Invalidate both
        string memKey = MemKey(id);
        string distKey = DistKey(id);

        _memory.Remove(memKey);
        await _dist.RemoveAsync(distKey, ct);
        _logger.LogInformation("[Hybrid] INVALIDATE product {Id}", id);

        // Optional write-through (faster next hit)
        try
        {
            Product? p = await _repo.GetByIdAsync(id, ct);
            if(p is null)
            {
                if(_settings.EnableNegativeCaching)
                {
                    Envelope<Product> envNull = Envelope<Product>.Null(
                        DateTime.UtcNow.AddSeconds(_settings.NullTtlSeconds));
                    await SetDistributedAsync(
                        distKey,
                        envNull,
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_settings.NullTtlSeconds)
                        },
                        ct);
                    SetMemory(memKey, null, ttlSeconds: _settings.NullTtlSeconds);
                    _logger.LogInformation("[Hybrid] SET NULL (WriteThrough) {Key}", distKey);
                }
                return;
            }

            Envelope<Product> envOk = Envelope<Product>.From(
                p,
                DateTime.UtcNow.AddSeconds(_settings.DistributedTtlSeconds));
            await SetDistributedAsync(distKey, envOk, _distOpts, ct);
            SetMemory(memKey, p);
            _logger.LogInformation("[Hybrid] SET (WriteThrough) {Key}", distKey);
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "[Hybrid] WriteThrough failed for {Id}", id);
        }
    }
}