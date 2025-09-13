namespace Memory_Cache;

public sealed class MemoryCacheSettings
{
    /// <summary>
    /// آستانه‌ی ی رفرش زودهنگام
    /// </summary>
    public int EarlyRefreshSeconds { get; init; } = 5;

    public bool EnableNegativeCaching { get; init; } = true;

    /// <summary>
    /// اگر SizeLimit برای IMemoryCache ست شده
    /// </summary>
    public int EntrySize { get; init; } = 1;

    /// <summary>
    /// TTL عادی
    /// </summary>
    public int MemoryTtlSeconds { get; init; } = 60;

    /// <summary>
    /// TTL برای null (negative caching)
    /// </summary>
    public int NullTtlSeconds { get; init; } = 10;
}