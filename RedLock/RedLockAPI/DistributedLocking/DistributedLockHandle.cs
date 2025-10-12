using RedLockNet;
namespace RedLockAPI.DistributedLocking;

public sealed class DistributedLockHandle : IAsyncDisposable
{
    private readonly IRedLock _redLock;
    private readonly Func<Task<bool>>? _renewFunc;
    private readonly CancellationTokenSource _cts = new();
    private readonly ILogger? _logger;
    private Task? _renewalTask;

    public bool IsAcquired => _redLock?.IsAcquired ?? false;
    public string Resource => _redLock?.Resource ?? string.Empty;
    public string LockId => _redLock?.LockId ?? string.Empty;

    internal DistributedLockHandle(IRedLock redLock , Func<Task<bool>>? renewFunc , TimeSpan renewalInterval , ILogger? logger)
    {
        _redLock = redLock ?? throw new ArgumentNullException(nameof(redLock));
        _renewFunc = renewFunc;
        _logger = logger;

        if ( _renewFunc != null && renewalInterval > TimeSpan.Zero )
        {
            _renewalTask = Task.Run(async () =>
            {
                try
                {
                    while ( !_cts.IsCancellationRequested )
                    {
                        await Task.Delay(renewalInterval , _cts.Token).ConfigureAwait(false);
                        if ( _cts.IsCancellationRequested )
                            break;

                        bool ok = false;
                        try
                        {
                            ok = await _renewFunc().ConfigureAwait(false);
                        }
                        catch ( Exception ex )
                        {
                            _logger?.LogError(ex , "Error during lock renewal for {Resource}" , Resource);
                        }

                        if ( !ok )
                        {
                            _logger?.LogWarning("Lock renewal failed for {Resource}. Stopping renewal loop." , Resource);
                            break;
                        }

                        _logger?.LogDebug("Lock renewed for {Resource}" , Resource);
                    }
                }
                catch ( TaskCanceledException ) { }
                catch ( Exception ex )
                {
                    _logger?.LogError(ex , "Unhandled renewal loop exception for {Resource}" , Resource);
                }
            });
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            _cts.Cancel();
            if ( _renewalTask != null )
            {
                try
                { await _renewalTask.ConfigureAwait(false); }
                catch { }
            }

            _redLock?.Dispose();
        }
        finally
        {
            _cts.Dispose();
        }
    }
}