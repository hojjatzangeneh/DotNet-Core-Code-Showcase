using RabbitMQ.Client;

using System;
using System.Linq;

using System.Text;

namespace RabbitMQ_Producer;

public class ProducerExchangeToExchange
{
    public async Task CallProducerAsync()
    {
        ConnectionFactory connectionFactory = new ConnectionFactory
        {
            HostName = "localhost"
        };
        const string firstExchange = "FirstExchange";
        const string secondExchange = "SecondExchange";
        const string routingKey = "ExchangeToExchange";// or Exchange2ToExchange
        await using IConnection connection = await connectionFactory.CreateConnectionAsync().ConfigureAwait(false);
        await using IChannel channel = await connection.CreateChannelAsync().ConfigureAwait(false);

        await channel.ExchangeDeclareAsync(firstExchange, type: ExchangeType.Direct).ConfigureAwait(false);
        await channel.ExchangeDeclareAsync(secondExchange, type: ExchangeType.Fanout).ConfigureAwait(false);

        await channel.ExchangeBindAsync(secondExchange, firstExchange, routingKey).ConfigureAwait(false);

        const string message = "This is a ExchangeToExchange message from Producer";
        byte[] body = Encoding.UTF8.GetBytes(message);

        await channel.BasicPublishAsync(
            exchange: firstExchange,
            routingKey: string.Empty,//routingKey
            mandatory: false,
            body: body,
            cancellationToken: default)
            .ConfigureAwait(false);

        Console.WriteLine("Your message sent");
        Console.ReadLine();
    }
}