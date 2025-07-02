using RabbitMQ.Client;

using RabbitMQ.Client.Events;

using System.Text;

namespace RabbitMQ_Consumer;

public class ConsumerHeaderExchange
{
    public async Task CallConsumerAsync()
    {
        ConnectionFactory connectionFactory = new()
        {
            HostName = "localhost"
        };

        using IConnection connection = await connectionFactory.CreateConnectionAsync().ConfigureAwait(false);
        using IChannel channel = await connection.CreateChannelAsync().ConfigureAwait(false);
        Dictionary<string, object?>? arguments =
                new()
        {
            { "format", "pdf" },
            { "type", "report" },
            { "x-match", "all" }
        };
        const string exchangeName = "HeaderExchange";
        const string routingKey = "";
        const string queueName = "HeaderQueue";

        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Headers,
            durable: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: default)
            .ConfigureAwait(false);

        await channel.QueueBindAsync(
            queue: queueName,
            exchange: exchangeName,
            routingKey: routingKey,
            arguments: arguments,
            noWait: false,
            cancellationToken: default)
            .ConfigureAwait(false);

        // تعریف مصرف کننده
        AsyncEventingBasicConsumer consumer = new(channel);

        consumer.ReceivedAsync += async (sender, ea) =>
        {
            byte[] body = ea.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"Received Header message: {message}");

            // اگر autoAck = true باشه نیازی نیست این رو بنویسید، ولی برای درک بهتر قرار دادم
            await Task.CompletedTask;
        };

        // شروع مصرف
        await channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: true, // Auto Acknowledge
            consumer: consumer)
            .ConfigureAwait(false);

        Console.WriteLine("Consumer is listening for Header messages...");
        Console.ReadLine();
    }
}