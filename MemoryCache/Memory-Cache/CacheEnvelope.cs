namespace Memory_Cache;

// ------------- Envelope برای متادیتا و کنترل refresh -------------
public sealed class CacheEnvelope
{
    int _refreshing = 0;

    CacheEnvelope(DateTimeOffset expiresAt)
    {
        Value = null;
        ExpiresAt = expiresAt;
        ValueIsNull = true;
    }

    public CacheEnvelope(object value, DateTimeOffset expiresAt)
    {
        Value = value;
        ExpiresAt = expiresAt;
        ValueIsNull = false;
    }

    public void EndRefresh()
    {
        Interlocked.Exchange(ref _refreshing, 0);
    }

    public static CacheEnvelope Null(DateTimeOffset expiresAt)
    {
        return new(expiresAt);
    }

    public bool TryBeginRefresh()
    {
        return Interlocked.CompareExchange(ref _refreshing, 1, 0) == 0;
    }

    public DateTimeOffset ExpiresAt { get; }

    public object? Value { get; }

    public bool ValueIsNull { get; }
}