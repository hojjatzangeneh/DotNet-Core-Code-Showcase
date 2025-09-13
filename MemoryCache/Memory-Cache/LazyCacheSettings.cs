namespace Memory_Cache;

public sealed class LazyCacheSettings
{
    public bool EnableNegativeCaching { get; init; } = true;

    public string KeyPrefix { get; init; } = "lazy:product:v1:";

    public int NullTtlSeconds { get; init; } = 15;

    public int ProductTtlSeconds { get; init; } = 120;
}