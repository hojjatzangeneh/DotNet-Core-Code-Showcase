using Confluent.Kafka;

using KafkaApp.Models;

using Microsoft.Extensions.Options;

namespace KafkaApp.Kafka;

public sealed class ConsumerWorker : BackgroundService
{
    readonly MessageBuffer _buffer;
    readonly KafkaSettings _settings;

    public ConsumerWorker(IOptions<KafkaSettings> opt, MessageBuffer buffer)
    {
        _settings = opt.Value;
        _buffer = buffer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ConsumerConfig conf = new()
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = _settings.GroupId,
            EnableAutoCommit = false,
            AutoOffsetReset =
                (_settings.AutoOffsetReset?.ToLower() == "earliest") ? AutoOffsetReset.Earliest : AutoOffsetReset.Latest,
            PartitionAssignmentStrategy = PartitionAssignmentStrategy.CooperativeSticky,
            ClientId = "kafka-dotnet-demo-consumer"
        };

        using IConsumer<string, string> consumer = new ConsumerBuilder<string, string>(conf)
        .SetErrorHandler((_, e) => Console.WriteLine($"[CONSUMER ERROR] {e.Reason}"))
            .SetPartitionsAssignedHandler(
                (c, parts) => Console.WriteLine($"[REBALANCE ASSIGNED] {string.Join("," , parts)}"))
            .SetPartitionsRevokedHandler(
                (c, parts) => Console.WriteLine($"[REBALANCE REVOKED] {string.Join("," , parts)}"))
            .Build();

        string topic = string.IsNullOrWhiteSpace(_settings.DefaultTopic) ? "test" : (_settings.DefaultTopic!);
        consumer.Subscribe(topic);
        Console.WriteLine($"[CONSUMER] Subscribed to '{topic}'");

        try
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    ConsumeResult<string, string> cr = consumer.Consume(stoppingToken);
                    if(cr is null)
                    {
                        continue;
                    }

                    _buffer.Add(
                        new ConsumedMessage
                        {
                            Topic = cr.Topic,
                            Partition = cr.Partition.Value,
                            Offset = cr.Offset.Value,
                            Key = cr.Message.Key,
                            Value = cr.Message.Value,
                            TimestampUtc = cr.Message.Timestamp.UtcDateTime
                        });

                    consumer.Commit(cr);
                }
                catch(ConsumeException ex)
                {
                    Console.WriteLine($"[CONSUME EX] {ex.Error.Reason}");
                }
            }
        }
        catch(OperationCanceledException)
        {
        }
        finally
        {
            try
            {
                consumer.Close();
            }
            catch
            {
            }
        }

        await Task.CompletedTask;
    }
}