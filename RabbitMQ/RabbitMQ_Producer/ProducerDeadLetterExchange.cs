using RabbitMQ.Client;

using System;
using System.Linq;

using System.Text;

namespace RabbitMQ_Producer;

public class ProducerDeadLetterExchange
{
    public async Task CallProducerAsync()
    {
        ConnectionFactory connectionFactory = new ConnectionFactory
        {
            HostName = "localhost"
        };

        await using IConnection connection = await connectionFactory.CreateConnectionAsync().ConfigureAwait(false);
        await using IChannel channel = await connection.CreateChannelAsync().ConfigureAwait(false);
        const string deadLetterExchangeName = "DeadLetterExchange";
        const string deadLetterQueueName = "DeadLetterQueue";
        const string queueName = "SafeQueue";
        const string routingKey = "errorMessage";
        await channel.ExchangeDeclareAsync(deadLetterExchangeName, type: ExchangeType.Direct).ConfigureAwait(false);
        await channel.QueueDeclareAsync(
            queue: deadLetterQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            passive: false,
            noWait: false,
            cancellationToken: default)
            .ConfigureAwait(true);
        await channel.QueueBindAsync(deadLetterQueueName, deadLetterExchangeName, routingKey).ConfigureAwait(false);
        Dictionary<string, object?> queueArguments = new Dictionary<string, object?>
        {
            { "x-dead-letter-exchange", deadLetterExchangeName },
            { "x-dead-letter-routing-key", routingKey },
            { "x-message-ttl", 100000 }
        };
        await channel.QueueDeclareAsync(queueName, true, false, false, arguments: queueArguments)
            .ConfigureAwait(false);

        const string message = "This is a Fanout DeadLetter from Producer";
        byte[] body = Encoding.UTF8.GetBytes(message);

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: queueName,
            mandatory: false,
            // basicProperties: null ,
            body: body,
            cancellationToken: default)
            .ConfigureAwait(false);

        Console.WriteLine("Your message has been sent to the consumer.");
        Console.ReadLine();
    }
}