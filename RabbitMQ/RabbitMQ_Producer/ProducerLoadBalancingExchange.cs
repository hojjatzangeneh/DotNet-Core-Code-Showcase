
using RabbitMQ.Client;

using System;
using System.Linq;

using System.Text;

namespace RabbitMQ_Producer;

public class ProducerLoadBalancingExchange
{
    public async Task CallProducerAsync()
    {
        ConnectionFactory connectionFactory = new ConnectionFactory
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
        for(int i = 0; i < 100; i++)
        {
            string message = $"Message ==> {i}";
            byte[] body = Encoding.UTF8.GetBytes(message);
            await channel.BasicPublishAsync(string.Empty, queueName, body, default).ConfigureAwait(false);
            Thread.Sleep(1500);
        }
        Console.ReadLine();
    }
}