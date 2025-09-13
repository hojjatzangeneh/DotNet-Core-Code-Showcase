namespace Memory_Cache;

public sealed class DistributedCacheSettings
{
    public int EarlyRefreshSeconds { get; init; } = 5;   // soft-ttl window for refresh-ahead

    public bool EnableCompression { get; init; } = true; // gzip payloads to save bandwidth

    public bool EnableNegativeCaching { get; init; } = true;

    public string KeyPrefix { get; init; } = "dist:product:v1:"; // bump version for bulk invalidation

    public int NullTtlSeconds { get; init; } = 15;       // TTL for negative caching (nulls)

    public int ProductTtlSeconds { get; init; } = 120;   // hard TTL in the distributed cache
}