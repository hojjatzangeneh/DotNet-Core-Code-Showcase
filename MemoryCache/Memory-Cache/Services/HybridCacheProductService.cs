using Memory_Cache.Models;

using Memory_Cache.Repositories;

using Microsoft.Extensions.Caching.Distributed;

using Microsoft.Extensions.Caching.Memory;

using System.Text.Json;

namespace Memory_Cache.Services;

public class HybridCacheProductService : IProductService
{
    static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    readonly IDistributedCache distributedCache;
    readonly DistributedCacheEntryOptions distributedCacheEntryOptions;
    readonly ILogger<HybridCacheProductService> logger;
    readonly IMemoryCache memoryCache;
    readonly IProductRepository productRepository;
    readonly TimeSpan timeSpan;

    public HybridCacheProductService(
        IMemoryCache memory,
        IDistributedCache distributed,
        IProductRepository repo,
        ILogger<HybridCacheProductService> logger,
        IConfiguration config)
    {
        memoryCache = memory;
        distributedCache = distributed;
        productRepository = repo;
        this.logger = logger;

        timeSpan = TimeSpan.FromSeconds(config.GetValue<int>("Cache:Defaults:MemoryTtlSeconds", 60));
        distributedCacheEntryOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow =
                TimeSpan.FromSeconds(config.GetValue<int>("Cache:Defaults:ProductTtlSeconds", 120))
        };
    }

    public async Task<Product?> GetAsync(int id, CancellationToken ct = default)
    {
        string memKey = $"hyb:mem:product:{id}";
        if(memoryCache.TryGetValue(memKey, out Product? p))
        {
            logger.LogInformation("[Hybrid] Memory HIT {Key}", memKey);
            return p;
        }

        string distKey = $"hyb:dist:product:{id}";
        string? json = await distributedCache.GetStringAsync(distKey, ct);
        if(json is not null)
        {
            logger.LogInformation("[Hybrid] Dist HIT {Key}", distKey);
            p = JsonSerializer.Deserialize<Product>(json, JsonOptions)!;
            // warm memory
            memoryCache.Set(memKey, p!, new MemoryCacheEntryOptions().SetSize(1).SetAbsoluteExpiration(timeSpan));
            return p;
        }

        // Load
        logger.LogInformation("[Hybrid] MISS => load from repository");
        p = await productRepository.GetByIdAsync(id, ct);
        if(p is null)
        {
            return null;
        }

        memoryCache.Set(memKey, p, new MemoryCacheEntryOptions().SetSize(1).SetAbsoluteExpiration(timeSpan));
        await distributedCache.SetStringAsync(
            distKey,
            JsonSerializer.Serialize(p, JsonOptions),
            distributedCacheEntryOptions,
            ct);
        return p;
    }

    public async Task UpdatePriceAsync(int id, decimal price, CancellationToken ct = default)
    {
        await productRepository.UpdatePriceAsync(id, price, ct);
        // Invalidate both
        memoryCache.Remove($"hyb:mem:product:{id}");
        await distributedCache.RemoveAsync($"hyb:dist:product:{id}", ct);
        logger.LogInformation("[Hybrid] INVALIDATE product {Id}", id);
    }
}