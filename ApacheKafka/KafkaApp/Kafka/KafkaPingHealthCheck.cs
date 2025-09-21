using Confluent.Kafka;

using KafkaApp.Kafka;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace KafkaApp.Kafka;

public sealed class KafkaPingHealthCheck : IHealthCheck
{
    readonly KafkaSettings _settings;

    public KafkaPingHealthCheck(IOptions<KafkaSettings> opt)
    {
        _settings = opt.Value;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using IAdminClient admin = new AdminClientBuilder(
                new AdminClientConfig { BootstrapServers = _settings.BootstrapServers }).Build();
            _ = admin.GetMetadata(TimeSpan.FromSeconds(3));
            return Task.FromResult(HealthCheckResult.Healthy("Kafka reachable"));
        }
        catch(Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy($"Kafka unreachable: {ex.Message}"));
        }
    }
}