
namespace KafkaApp.Models;

public sealed class ConsumedMessage
{
    public string? Key { get; init; }

    public long Offset { get; init; }

    public int Partition { get; init; }

    public DateTime TimestampUtc { get; init; }

    public string Topic { get; init; }

    public string Value { get; init; } = string.Empty;
}