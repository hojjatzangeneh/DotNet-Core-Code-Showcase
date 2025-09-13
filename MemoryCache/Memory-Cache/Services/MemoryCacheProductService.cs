using Memory_Cache.Models;
using Memory_Cache.Repositories;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using System.Collections.Concurrent;

namespace Memory_Cache.Services;

// ------------- Service -------------
public class MemoryCacheProductService(
    IMemoryCache memory,
    IProductRepository repo,
    ILogger<MemoryCacheProductService> logger,
    IOptions<MemoryCacheSettings> options) : IProductService
{
    // Key versioning
    const string KeyPrefix = "mem:product:v1:";

    // Per-key lock to prevent cache stampede
    static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    readonly MemoryCacheSettings _settings = options.Value ?? new MemoryCacheSettings();

    // ----------------- Helpers -----------------
    static string BuildKey(int id)
    {
        return $"{KeyPrefix}{id}";
    }

    static SemaphoreSlim GetLock(string key)
    {
        return _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
    }

    async Task RefreshInBackgroundAsync(int id, string key, CancellationToken ct)
    {
        try
        {
            SemaphoreSlim gate = GetLock(key);
            if(!(await gate.WaitAsync(0, ct)))
            {
                return; // Someone else is refreshing
            }

            try
            {
                Product? product = await repo.GetByIdAsync(id, ct);
                DateTimeOffset now = DateTimeOffset.UtcNow;

                if(product is null)
                {
                    if(_settings.EnableNegativeCaching)
                    {
                        SetEnvelope(
                            key,
                            CacheEnvelope.Null(now.AddSeconds(_settings.NullTtlSeconds)),
                            TimeSpan.FromSeconds(_settings.NullTtlSeconds));
                        logger.LogInformation("[Memory] REFRESH → NULL {Key}", key);
                    }
                    else
                    {
                        memory.Remove(key);
                    }
                    return;
                }

                SetEnvelope(
                    key,
                    new CacheEnvelope(product, now.AddSeconds(_settings.MemoryTtlSeconds)),
                    TimeSpan.FromSeconds(_settings.MemoryTtlSeconds));
                logger.LogInformation("[Memory] REFRESH → SET {Key}", key);
            }
            finally
            {
                gate.Release();
            }
        }
        catch(Exception ex)
        {
            logger.LogWarning(ex, "[Memory] REFRESH failed for {Key}", key);
        }
        finally
        {
            // Clear Refreshing flag if still in cache
            if(memory.TryGetValue(key, out CacheEnvelope? env))
            {
                env?.EndRefresh();
            }
        }
    }

    void SetEnvelope(string key, CacheEnvelope env, TimeSpan ttl)
    {
        MemoryCacheEntryOptions opts = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(ttl)
            .SetPriority(CacheItemPriority.Normal)
            .SetSize(_settings.EntrySize)
            .RegisterPostEvictionCallback(
                (k, _, reason, __) =>
                logger.LogInformation("[Memory] EVICT {Key} due to {Reason}", k, reason));

        memory.Set(key, env, opts);
    }

    public async Task<Product?> GetAsync(int id, CancellationToken ct = default)
    {
        string key = BuildKey(id);
        DateTimeOffset now = DateTimeOffset.UtcNow;

        // 1) Try from memory
        if(memory.TryGetValue(key, out CacheEnvelope envelope))
        {
            logger.LogInformation("[Memory] HIT {Key}", key);

            // If envelope indicates null
            if(envelope.ValueIsNull)
            {
                return null;
            }

            // Early refresh (stale-while-revalidate)
            TimeSpan remaining = envelope.ExpiresAt - now;
            if(!_settings.EarlyRefreshSeconds.Equals(0) &&
                (remaining <= TimeSpan.FromSeconds(_settings.EarlyRefreshSeconds)) &&
                envelope.TryBeginRefresh())
            {
                _ = RefreshInBackgroundAsync(id, key, ct);
            }

            return (Product) envelope.Value!;
        }

        logger.LogInformation("[Memory] MISS {Key}", key);

        // 2) Prevent stampede: acquire per-key lock
        SemaphoreSlim gate = GetLock(key);
        await gate.WaitAsync(ct);
        try
        {
            // Double-check after acquiring lock
            if(memory.TryGetValue(key, out envelope))
            {
                logger.LogInformation("[Memory] HIT2 {Key}", key);
                return envelope.ValueIsNull ? null : ((Product) envelope.Value!);
            }

            // 3) Fetch from source of truth
            Product? product = await repo.GetByIdAsync(id, ct);

            // 4) Negative caching for null
            if(product is null)
            {
                if(_settings.EnableNegativeCaching)
                {
                    SetEnvelope(
                        key,
                        CacheEnvelope.Null(now.AddSeconds(_settings.NullTtlSeconds)),
                        TimeSpan.FromSeconds(_settings.NullTtlSeconds));
                    logger.LogInformation("[Memory] SET NULL {Key}", key);
                }
                return null;
            }

            // 5) Normal caching
            SetEnvelope(
                key,
                new CacheEnvelope(product, now.AddSeconds(_settings.MemoryTtlSeconds)),
                TimeSpan.FromSeconds(_settings.MemoryTtlSeconds));
            logger.LogInformation("[Memory] SET {Key}", key);

            return product;
        }
        finally
        {
            gate.Release();
        }
    }

    // Manual invalidation if needed
    public void Invalidate(int id)
    {
        string key = BuildKey(id);
        memory.Remove(key);
        logger.LogInformation("[Memory] INVALIDATE(manual) {Key}", key);
    }

    public async Task UpdatePriceAsync(int id, decimal price, CancellationToken ct = default)
    {
        await repo.UpdatePriceAsync(id, price, ct);

        // 1) Invalidate
        string key = BuildKey(id);
        memory.Remove(key);
        logger.LogInformation("[Memory] INVALIDATE {Key}", key);

        // 2) (Optional) Write-through → cache fresh value so next HIT is immediate
        try
        {
            Product? product = await repo.GetByIdAsync(id, ct);
            DateTimeOffset now = DateTimeOffset.UtcNow;

            if(product is null)
            {
                if(_settings.EnableNegativeCaching)
                {
                    SetEnvelope(
                        key,
                        CacheEnvelope.Null(now.AddSeconds(_settings.NullTtlSeconds)),
                        TimeSpan.FromSeconds(_settings.NullTtlSeconds));
                    logger.LogInformation("[Memory] SET NULL (WriteThrough) {Key}", key);
                }
                return;
            }

            SetEnvelope(
                key,
                new CacheEnvelope(product, now.AddSeconds(_settings.MemoryTtlSeconds)),
                TimeSpan.FromSeconds(_settings.MemoryTtlSeconds));
            logger.LogInformation("[Memory] SET (WriteThrough) {Key}", key);
        }
        catch(Exception ex)
        {
            // On error, just invalidate; next request will fetch from source
            logger.LogWarning(ex, "[Memory] WriteThrough failed for {Key}", key);
        }
    }

    // Simple group invalidation (e.g., all products) by bumping key version: change KeyPrefix to v2.
    // This avoids iterating over all keys and is cheaper.
}