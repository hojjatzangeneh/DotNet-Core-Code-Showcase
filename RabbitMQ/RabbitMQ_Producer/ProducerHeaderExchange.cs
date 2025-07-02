using RabbitMQ.Client;

using System;
using System.Linq;

using System.Text;

namespace RabbitMQ_Producer;

public class ProducerHeaderExchange
{
    public async Task CallProducerAsync()
    {
        ConnectionFactory connectionFactory = new ConnectionFactory
        {
            HostName = "localhost"
        };

        await using IConnection connection = await connectionFactory.CreateConnectionAsync().ConfigureAwait(false);
        await using IChannel channel = await connection.CreateChannelAsync().ConfigureAwait(false);
        BasicProperties basicProperties = new()
        {
            ContentType = "text/plain",
            DeliveryMode = DeliveryModes.Persistent,
            Headers =
                new Dictionary<string, object?>
                {
                { "format", "pdf" },
                { "type", "report" },
                { "x-match", "all" }
                }
        };
        const string exchangeName = "HeaderExchange";
        const string routingKey = "";

        // 1. تعریف Exchange از نوع Header
        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Headers,
            durable: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: default)
            .ConfigureAwait(false);

        // 2. ارسال پیام به exchange
        const string message = "This is a header message from Producer";
        byte[] body = Encoding.UTF8.GetBytes(message);

        await channel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: basicProperties,
            body: body,
            cancellationToken: default)
            .ConfigureAwait(false);

        Console.WriteLine("Your message has been sent to the consumer.");
        Console.ReadLine();
    }
}