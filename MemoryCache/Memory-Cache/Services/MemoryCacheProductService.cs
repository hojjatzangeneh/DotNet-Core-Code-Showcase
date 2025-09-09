using Memory_Cache.Models;
using Memory_Cache.Repositories;

using Microsoft.Extensions.Caching.Memory;

namespace Memory_Cache.Services;

public class MemoryCacheProductService(
    IMemoryCache memory ,
    IProductRepository repo ,
    ILogger<MemoryCacheProductService> logger ,
    IConfiguration config) : IProductService
{
    private readonly TimeSpan timeSpan = TimeSpan.FromSeconds(config.GetValue<int>("Cache:Defaults:MemoryTtlSeconds" , 60));

    public async Task<Product?> GetAsync(int id , CancellationToken ct = default)
    {
        string key = $"mem:product:{id}";
        if ( memory.TryGetValue(key , out Product? cached) )
        {
            logger.LogInformation("[Memory] HIT for {Key}" , key);
            return cached;
        }

        logger.LogInformation("[Memory] MISS for {Key}" , key);
        Product? p = await repo.GetByIdAsync(id , ct);
        if ( p is null )
        {
            return null;
        }

        MemoryCacheEntryOptions opts = new MemoryCacheEntryOptions()
            .SetSize(1)
            .SetAbsoluteExpiration(timeSpan)
            .SetPriority(CacheItemPriority.Normal)
            .RegisterPostEvictionCallback(
                (k , _ , reason , __) => logger.LogInformation("[Memory] EVICT {Key} due to {Reason}" , k , reason));

        memory.Set(key , p , opts);
        return p;
    }

    public async Task UpdatePriceAsync(int id , decimal price , CancellationToken ct = default)
    {
        await repo.UpdatePriceAsync(id , price , ct);

        // invalidate
        string key = $"mem:product:{id}";
        memory.Remove(key);
        logger.LogInformation("[Memory] INVALIDATE {Key}" , key);
    }
}