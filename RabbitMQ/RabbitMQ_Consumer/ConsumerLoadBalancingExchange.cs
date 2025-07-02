using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;

using System.Linq;
using System.Text;

namespace RabbitMQ_Consumer;

public class ConsumerLoadBalancingExchange
{
    public async Task CallConsumerAsync()
    {
        ConnectionFactory connectionFactory = new()
        {
            HostName = "localhost"
        };
        await using IConnection connection = await connectionFactory.CreateConnectionAsync().ConfigureAwait(false);
        await using IChannel channel = await connection.CreateChannelAsync().ConfigureAwait(false);

        const string queueName = "LoadBalancingQueue";

        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: false,
            exclusive: false,
            arguments: null,
            noWait: false,
            cancellationToken: default)
            .ConfigureAwait(false);

        // تعریف مصرف کننده
        AsyncEventingBasicConsumer consumerEvent = new(channel);

        consumerEvent.ReceivedAsync += async (sender, ea) =>
        {
            byte[] body = ea.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"Received direct message: {message}");
            await Task.CompletedTask;
        };

        // شروع مصرف
        await channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: true,
            consumer: consumerEvent)
            .ConfigureAwait(false);

        Console.WriteLine("Consumer is listening for LoadBalancing messages...");
        Console.ReadLine();
    }
}