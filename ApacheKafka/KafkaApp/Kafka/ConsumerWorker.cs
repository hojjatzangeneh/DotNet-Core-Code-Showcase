using Confluent.Kafka;

using KafkaApp.Models;

using Microsoft.Extensions.Options;

namespace KafkaApp.Kafka;

public sealed class ConsumerWorker(IOptions<KafkaSettings> opt, MessageBuffer buffer) : BackgroundService
{
    readonly KafkaSettings _settings = opt.Value;

    void RunLoop(CancellationToken stoppingToken)
    {
        ConsumerConfig conf = new()
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = _settings.GroupId,
            EnableAutoCommit = false,
            AutoOffsetReset =
                (_settings.AutoOffsetReset?.ToLower() == "earliest")
                    ? AutoOffsetReset.Earliest
                    : AutoOffsetReset.Latest,
            PartitionAssignmentStrategy = PartitionAssignmentStrategy.CooperativeSticky,
            ClientId = "kafka-dotnet-demo-consumer",
            SocketKeepaliveEnable = true
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

                    buffer.Add(
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
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Factory
            .StartNew(
                () => RunLoop(stoppingToken),
                stoppingToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
    }
}