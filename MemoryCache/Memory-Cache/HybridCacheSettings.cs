namespace Memory_Cache;

public sealed class HybridCacheSettings
{
    public string DistKeyPrefix { get; init; } = "hyb:dist:product:v1:";

    public int DistributedTtlSeconds { get; init; } = 120;

    public int EarlyRefreshSeconds { get; init; } = 5;

    public bool EnableCompression { get; init; } = true;

    public bool EnableNegativeCaching { get; init; } = true;

    /// <summary>
    /// When SizeLimit is enable
    /// </summary>
    public int MemoryEntrySize { get; init; } = 1;

    public string MemoryKeyPrefix { get; init; } = "hyb:mem:product:v1:";

    public int MemoryTtlSeconds { get; init; } = 60;

    public int NullTtlSeconds { get; init; } = 15;
}