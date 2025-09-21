
namespace KafkaApp.Models;

public sealed class CreateTopicRequest
{
    public string Name { get; set; } = string.Empty;

    public int Partitions { get; set; } = 1;

    public int ReplicationFactor { get; set; } = 1; // در محیط تک‌بروکر 1 بماند
}