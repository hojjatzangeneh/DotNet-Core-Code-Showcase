using LazyCache;

using Memory_Cache.Models;
using Memory_Cache.Repositories;

using Microsoft.Extensions.Options;

namespace Memory_Cache.Services;

public class LazyCacheProductService(
    IAppCache cache,
    IProductRepository repo,
    ILogger<LazyCacheProductService> logger,
    IOptions<LazyCacheSettings> options) : IProductService
{
    static readonly object NullMarker = new(); // used for negative caching
    readonly LazyCacheSettings _settings = options.Value ?? new LazyCacheSettings();

    string BuildKey(int id)
    {
        return $"{_settings.KeyPrefix}{id}";
    }

    public async Task<Product?> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        string key = BuildKey(id);

        return await cache.GetOrAddAsync(
            key,
            async entry =>
            {
                logger.LogInformation("[LazyCache] Loader executing for {Key}", key);

                Product? product = await repo.GetByIdAsync(id, cancellationToken);

                if((product is null) && _settings.EnableNegativeCaching)
                {
                    // store null marker with shorter TTL
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_settings.NullTtlSeconds);
                    logger.LogInformation("[LazyCache] Caching NULL for {Key}", key);
                    return NullMarker;
                }

                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_settings.ProductTtlSeconds);
                return product ?? NullMarker;
            }) switch
        {
            Product p => p,
            _ => null // translate NullMarker back to null
        };
    }

    public async Task UpdatePriceAsync(int id, decimal price, CancellationToken cancellationToken = default)
    {
        await repo.UpdatePriceAsync(id, price, cancellationToken);

        string key = BuildKey(id);
        cache.Remove(key);
        logger.LogInformation("[LazyCache] INVALIDATE {Key}", key);

        // optional write-through: immediately re-cache
        try
        {
            Product? product = await repo.GetByIdAsync(id, cancellationToken);
            if(product is not null)
            {
                cache.Add(key, product, TimeSpan.FromSeconds(_settings.ProductTtlSeconds));
                logger.LogInformation("[LazyCache] SET (WriteThrough) {Key}", key);
            }
            else if(_settings.EnableNegativeCaching)
            {
                cache.Add(key, NullMarker, TimeSpan.FromSeconds(_settings.NullTtlSeconds));
                logger.LogInformation("[LazyCache] SET NULL (WriteThrough) {Key}", key);
            }
        }
        catch(Exception ex)
        {
            logger.LogWarning(ex, "[LazyCache] WriteThrough failed for {Key}", key);
        }
    }

    // manual bulk invalidation by bumping KeyPrefix version in config (v1 → v2)
}