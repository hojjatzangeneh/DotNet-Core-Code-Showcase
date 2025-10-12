using Microsoft.Extensions.Options;

using RedLockNet;
using RedLockNet.SERedis;

using StackExchange.Redis;

namespace RedLockAPI.DistributedLocking;

public class DistributedLockService : IDistributedLockService, IAsyncDisposable
{
    private readonly RedLockFactory _factory;
    private readonly RedLockOptions _options;
    private readonly ILogger<DistributedLockService> _logger;
    private readonly IReadOnlyList<ConnectionMultiplexer> _connections;
    private readonly int _quorum;

    public DistributedLockService(RedLockFactory factory ,
                                  IOptions<RedLockOptions> options ,
                                  ILogger<DistributedLockService> logger ,
                                  IReadOnlyList<ConnectionMultiplexer> connections)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
        _connections = connections ?? throw new ArgumentNullException(nameof(connections));
        if ( _connections.Count == 0 )
            throw new ArgumentException("At least one ConnectionMultiplexer required" , nameof(connections));
        _quorum = ( _connections.Count / 2 ) + 1;
    }

    public async Task<DistributedLockHandle> TryAcquireLockAsync(string resource , CancellationToken cancellationToken = default)
    {
        if ( string.IsNullOrWhiteSpace(resource) )
            throw new ArgumentNullException(nameof(resource));

        var redLock = await _factory.CreateLockAsync(
            resource: resource ,
            expiryTime: _options.LockExpiry ,
            waitTime: _options.WaitTimeout ,
            retryTime: _options.RetryInterval ,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        if ( redLock == null || !redLock.IsAcquired )
        {
            _logger?.LogInformation("Could not acquire lock for {Resource}" , resource);
            // return a dummy non-acquired handle (using a small fake IRedLock)
            return new DistributedLockHandle(new RedLockWrapperFalse(resource) , null , TimeSpan.Zero , _logger);
        }

        Func<Task<bool>>? renewFunc = null;
        TimeSpan renewalInterval = TimeSpan.Zero;

        if ( _options.EnableAutomaticRenewal )
        {
            renewalInterval = _options.LockExpiry - _options.RenewalBuffer;
            if ( renewalInterval <= TimeSpan.Zero )
                renewalInterval = TimeSpan.FromTicks(_options.LockExpiry.Ticks / 3);

            // Lua: check value equals lockId then pexpire (ttl in ms)
            const string extendScript = @"
if redis.call('get', KEYS[1]) == ARGV[1] then
  return redis.call('pexpire', KEYS[1], ARGV[2])
else
  return 0
end
";

            renewFunc = async () =>
            {
                try
                {
                    var key = redLock.Resource;
                    var lockId = redLock.LockId;
                    var ttlMs = ( long ) _options.LockExpiry.TotalMilliseconds;

                    var tasks = _connections.Select(async conn =>
                    {
                        try
                        {
                            var db = conn.GetDatabase();
                            var res = await db.ScriptEvaluateAsync(extendScript , new RedisKey[] { key } , new RedisValue[] { lockId , ttlMs }).ConfigureAwait(false);
                            if ( res.IsNull )
                                return 0L;
                            return ( long ) res;
                        }
                        catch ( Exception ex )
                        {
                            _logger?.LogDebug(ex , "Extend script error on connection {Config}" , conn.Configuration);
                            return 0L;
                        }
                    }).ToArray();

                    var results = await Task.WhenAll(tasks).ConfigureAwait(false);
                    var success = results.Count(r => r > 0);
                    var ok = success >= _quorum;
                    if ( !ok )
                        _logger?.LogWarning("Lock renewal quorum not met ({Success}/{Total}) for {Resource}" , success , _connections.Count , key);
                    return ok;
                }
                catch ( Exception ex )
                {
                    _logger?.LogError(ex , "Exception during lock renewal for {Resource}" , redLock.Resource);
                    return false;
                }
            };
        }

        return new DistributedLockHandle(redLock , renewFunc , renewalInterval , _logger);
    }

    public ValueTask DisposeAsync()
    {
        try
        { _factory.Dispose(); }
        catch { }
        // Do not dispose ConnectionMultiplexer here if they are shared elsewhere by the app.
        return ValueTask.CompletedTask;
    }

    // minimal fake IRedLock to represent not-acquired lock — only what's used by handle
    private sealed class RedLockWrapperFalse : RedLockNet.IRedLock
    {
        private readonly string _resource;
        public RedLockWrapperFalse(string resource) => _resource = resource;
        public string Resource => _resource;
        public string LockId => string.Empty;
        public bool IsAcquired => false;
        public RedLockStatus Status => RedLockStatus.Unlocked;
        public RedLockInstanceSummary InstanceSummary => new RedLockInstanceSummary(0 , 0 , 0);
        public int ExtendCount => 0;

        public void Dispose()
        {
           // throw new NotImplementedException();
        }

        public ValueTask DisposeAsync()
        {
            return  ValueTask.CompletedTask;
        }
    }
}