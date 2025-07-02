using RabbitMQ.Client;

using System;
using System.Linq;

using System.Text;

namespace RabbitMQ_Producer;

public class ProducerFanoutExchange
{
    public async Task CallProducerAsync()
    {
        ConnectionFactory connectionFactory = new ConnectionFactory
        {
            HostName = "localhost"
        };

        await using IConnection connection = await connectionFactory.CreateConnectionAsync().ConfigureAwait(false);
        await using IChannel channel = await connection.CreateChannelAsync().ConfigureAwait(false);

        const string exchangeName = "FanoutExchange";
        const string routingKey = "";

        // 1. تعریف Exchange از نوع Fanout
        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Fanout,
            durable: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: default)
            .ConfigureAwait(false);

        // 2. ارسال پیام به exchange
        const string message = "This is a Fanout message from Producer";
        byte[] body = Encoding.UTF8.GetBytes(message);

        await channel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: routingKey,
            mandatory: false,
            // basicProperties: null ,
            body: body,
            cancellationToken: default)
            .ConfigureAwait(false);

        Console.WriteLine("Your message has been sent to the consumer.");
        Console.ReadLine();
    }
}