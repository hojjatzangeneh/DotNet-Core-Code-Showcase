namespace RedLockAPI.DistributedLocking;

public interface IDistributedLockService
{
    /// <summary>
    /// Acquire a lock for the given resource. Returned handle should be disposed (await using) to release the lock.
    /// </summary>
    Task<DistributedLockHandle> TryAcquireLockAsync(string resource , CancellationToken cancellationToken = default);
}