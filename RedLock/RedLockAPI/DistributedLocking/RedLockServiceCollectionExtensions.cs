using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;

using StackExchange.Redis;

namespace RedLockAPI.DistributedLocking;

public static class RedLockServiceCollectionExtensions
{
    public static async Task AddRedLockDistributedLockingAsync(this IServiceCollection services , IConfiguration configuration , ILogger? startupLogger = null)
    {
        var options = new RedLockOptions();
        var section = configuration.GetSection("RedLock");
        section.Bind(options);
        services.Configure<RedLockOptions>(section);

        if ( options.Endpoints == null || options.Endpoints.Count == 0 )
            throw new InvalidOperationException("Configure at least one Redis endpoint under RedLock:Endpoints");

        startupLogger?.LogInformation("Initializing RedLock with {Count} endpoints..." , options.Endpoints.Count);

        var connectionMultiplexers = new List<ConnectionMultiplexer>();
        var redlockMultiplexers = new List<RedLockMultiplexer>();

        foreach ( var ep in options.Endpoints )
        {
            startupLogger?.LogInformation("Connecting to {Endpoint}" , ep);

            var cfg = ConfigurationOptions.Parse(ep);
            cfg.AbortOnConnectFail = false;
            cfg.ConnectRetry = 3;
            cfg.ConnectTimeout = 5000;
            cfg.KeepAlive = 60;

            var conn = await ConnectionMultiplexer.ConnectAsync(cfg).ConfigureAwait(false);
            connectionMultiplexers.Add(conn);
            redlockMultiplexers.Add(new RedLockMultiplexer(conn));
        }

        var factory = RedLockFactory.Create(redlockMultiplexers);

        // register ConnectionMultiplexers as readonly list and factory
        services.AddSingleton<IReadOnlyList<ConnectionMultiplexer>>(connectionMultiplexers);
        services.AddSingleton(factory);
        services.AddSingleton<IDistributedLockService , DistributedLockService>();

        startupLogger?.LogInformation("RedLock Registered.");
    }
}