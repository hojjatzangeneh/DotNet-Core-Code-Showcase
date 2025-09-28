
namespace KafkaApp.Kafka;

public sealed class KafkaSettings
{
    public string? Acks { get; set; } = "all";

    public string? AutoOffsetReset { get; set; } = "Earliest";

    public int? BatchSizeBytes { get; set; } = 65536;

    public string BootstrapServers { get; set; } = "localhost:9092";

    public string? Compression { get; set; } = "snappy";

    public int ConsumeBufferCapacity { get; set; } = 500;

    public string? DefaultTopic { get; set; } = "test";

    public bool EnableIdempotence { get; set; } = true;

    public string GroupId { get; set; } = "demo-consumer-group";

    public int? LingerMs { get; set; } = 20;
}