using LazyCache;

using Memory_Cache.Models;
using Memory_Cache.Repositories;

namespace Memory_Cache.Services;

public class LazyCacheProductService(
    IAppCache cache,
    IProductRepository repo,
    ILogger<LazyCacheProductService> logger,
    IConfiguration config) : IProductService
{
    readonly TimeSpan timeSpan = TimeSpan.FromSeconds(config.GetValue<int>("Cache:Defaults:ProductTtlSeconds", 120));

    public async Task<Product?> GetAsync(int id, CancellationToken ct = default)
    {
        string key = $"lazy:product:{id}";
        return await cache.GetOrAddAsync(
            key,
            async () =>
            {
                logger.LogInformation("[LazyCache] loader executing for {Key}", key);
                return await repo.GetByIdAsync(id, ct);
            },
            timeSpan);
    }

    public async Task UpdatePriceAsync(int id, decimal price, CancellationToken ct = default)
    {
        await repo.UpdatePriceAsync(id, price, ct);
        string key = $"lazy:product:{id}";
        cache.Remove(key);
        logger.LogInformation("[LazyCache] INVALIDATE {Key}", key);
    }
}