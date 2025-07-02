using RabbitMQ.Client;

using RabbitMQ.Client.Events;

using System.Text;

namespace RabbitMQ_Consumer;

public class ConsumerTopicExchange
{
    public async Task CallConsumerAsync()
    {
        ConnectionFactory connectionFactory = new()
        {
            HostName = "localhost"
        };

        using IConnection connection = await connectionFactory.CreateConnectionAsync().ConfigureAwait(false);
        using IChannel channel = await connection.CreateChannelAsync().ConfigureAwait(false);

        const string exchangeName = "TopicExchange";
        const string routingKey = "hojjat.*.zangeneh";
        const string queueName = "TopicQueue";

        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Topic,
            durable: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: default)
            .ConfigureAwait(false);

        await channel.QueueBindAsync(
            queue: queueName,
            exchange: exchangeName,
            routingKey: routingKey,
            arguments: null,
            noWait: false,
            cancellationToken: default)
            .ConfigureAwait(false);

        // تعریف مصرف کننده
        AsyncEventingBasicConsumer consumer = new(channel);

        consumer.ReceivedAsync += async (sender, ea) =>
        {
            byte[] body = ea.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"Received topic message: {message}");

            // اگر autoAck = true باشه نیازی نیست این رو بنویسید، ولی برای درک بهتر قرار دادم
            await Task.CompletedTask;
        };

        // شروع مصرف
        await channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: true, // Auto Acknowledge
            consumer: consumer)
            .ConfigureAwait(false);

        Console.WriteLine("Consumer is listening for topic messages...");
        Console.ReadLine();
    }
}