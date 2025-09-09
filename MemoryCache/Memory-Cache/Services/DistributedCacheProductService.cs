using Memory_Cache.Models;

using Memory_Cache.Repositories;

using Microsoft.Extensions.Caching.Distributed;

using System.Text.Json;

namespace Memory_Cache.Services;

public class DistributedCacheProductService : IProductService
{
    static readonly JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web);
    readonly IDistributedCache distributedCache;
    readonly DistributedCacheEntryOptions distributedCacheEntryOptions;
    readonly ILogger<DistributedCacheProductService> logger;
    readonly IProductRepository productRepository;

    public DistributedCacheProductService(
        IDistributedCache distributed,
        IProductRepository repo,
        ILogger<DistributedCacheProductService> logger,
        IConfiguration config)
    {
        distributedCache = distributed;
        productRepository = repo;
        this.logger = logger;
        TimeSpan ttl = TimeSpan.FromSeconds(config.GetValue<int>("Cache:Defaults:ProductTtlSeconds", 120));
        distributedCacheEntryOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl };
    }

    public async Task<Product?> GetAsync(int id, CancellationToken ct = default)
    {
        string key = $"dist:product:{id}";
        string? cached = await distributedCache.GetStringAsync(key, ct);
        if(cached is not null)
        {
            logger.LogInformation("[Distributed] HIT for {Key}", key);
            return JsonSerializer.Deserialize<Product>(cached, jsonSerializerOptions);
        }

        logger.LogInformation("[Distributed] MISS for {Key}", key);
        Product? p = await productRepository.GetByIdAsync(id, ct);
        if(p is null)
        {
            return null;
        }

        string payload = JsonSerializer.Serialize(p, jsonSerializerOptions);
        await distributedCache.SetStringAsync(key, payload, distributedCacheEntryOptions, ct);
        return p;
    }

    public async Task UpdatePriceAsync(int id, decimal price, CancellationToken ct = default)
    {
        await productRepository.UpdatePriceAsync(id, price, ct);
        // invalidate
        string key = $"dist:product:{id}";
        await distributedCache.RemoveAsync(key, ct);
        logger.LogInformation("[Distributed] INVALIDATE {Key}", key);
    }
}