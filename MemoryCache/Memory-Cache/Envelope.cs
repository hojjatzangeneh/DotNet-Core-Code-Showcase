namespace Memory_Cache;

// Envelope keeps value + soft-expiry + a tiny refresh flag
public sealed class Envelope <T>
{
    // not serialized: local flag to avoid duplicate refresh on this node
    int _refreshing = 0;

    public void EndRefresh()
    {
        Interlocked.Exchange(ref _refreshing, 0);
    }

    public static Envelope<T> From(T value, DateTime expiresAtUtc)
    {
        return new() { Value = value, IsNull = false, ExpiresAtUtc = expiresAtUtc };
    }

    public static Envelope<T> Null(DateTime expiresAtUtc)
    {
        return new() { Value = default, IsNull = true, ExpiresAtUtc = expiresAtUtc };
    }

    public bool TryBeginRefresh()
    {
        return Interlocked.CompareExchange(ref _refreshing, 1, 0) == 0;
    }

    public DateTime ExpiresAtUtc { get; init; }

    public bool IsNull { get; init; }

    public T? Value { get; init; }
}