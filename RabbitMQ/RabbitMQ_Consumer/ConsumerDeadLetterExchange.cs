using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;

using System.Linq;
using System.Text;

namespace RabbitMQ_Consumer;

public class ConsumerDeadLetterExchange
{
    public async Task CallConsumerAsync()
    {
        ConnectionFactory connectionFactory = new()
        {
            HostName = "localhost"
        };
        await using IConnection connection = await connectionFactory.CreateConnectionAsync().ConfigureAwait(false);
        await using IChannel channel = await connection.CreateChannelAsync().ConfigureAwait(false);

        const string deadLetterExchangeName = "DeadLetterExchange";
        const string routingKey = "errorMessage";
        const string queueName = "SafeQueue";
        Dictionary<string, object?> arguments = new()
        {
            { "x-dead-letter-exchange", deadLetterExchangeName },
            { "x-dead-letter-routing-key", routingKey },
            { "x-message-ttl", 100000 }
        };
        await channel.QueueDeclareAsync(queueName, true, false, false, arguments: arguments)
            .ConfigureAwait(false);
        // تعریف مصرف کننده
        AsyncEventingBasicConsumer consumerEvent = new(channel);

        consumerEvent.ReceivedAsync += async (sender, ea) =>
        {
            byte[] body = ea.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"Received DeadLetter message: {message}");
            await channel.BasicRejectAsync(ea.DeliveryTag, false).ConfigureAwait(false);
            await Task.CompletedTask;
        };

        // شروع مصرف
        await channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: consumerEvent)
            .ConfigureAwait(false);

        Console.WriteLine("Consumer is listening for LoadBalancing messages...");
        Console.ReadLine();
    }
}