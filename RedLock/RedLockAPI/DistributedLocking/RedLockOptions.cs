namespace RedLockAPI.DistributedLocking;

public class RedLockOptions
{
    public List<string> Endpoints { get; set; } = new();
    public TimeSpan LockExpiry { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan WaitTimeout { get; set; } = TimeSpan.FromSeconds(10);
    public TimeSpan RetryInterval { get; set; } = TimeSpan.FromMilliseconds(200);

    // When true, the service will attempt to renew locks periodically until disposed
    public bool EnableAutomaticRenewal { get; set; } = true;

    // How early before expiry to attempt a renewal (e.g. 1/3 of expiry)
    public TimeSpan RenewalBuffer { get; set; } = TimeSpan.FromSeconds(10);
}
