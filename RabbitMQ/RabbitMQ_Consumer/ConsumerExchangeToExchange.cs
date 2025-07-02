using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;

using System.Linq;
using System.Text;

namespace RabbitMQ_Consumer;

public class ConsumerExchangeToExchange
{
    public async Task CallProducerAsync()
    {
        ConnectionFactory connectionFactory = new ConnectionFactory
        {
            HostName = "localhost"
        };
        const string secondExchange = "SecondExchange";
        const string queueName = "ExchangeToExchangeQueue";
        await using IConnection connection = await connectionFactory.CreateConnectionAsync().ConfigureAwait(false);
        await using IChannel channel = await connection.CreateChannelAsync().ConfigureAwait(false);

        await channel.ExchangeDeclareAsync(secondExchange, type: ExchangeType.Fanout).ConfigureAwait(false);
        await channel.QueueDeclareAsync(queueName, false, false, false, null, false, default)
            .ConfigureAwait(false);
        await channel.QueueBindAsync(queueName, secondExchange, string.Empty).ConfigureAwait(false);

        AsyncEventingBasicConsumer consumerEvent = new AsyncEventingBasicConsumer(channel);
        consumerEvent.ReceivedAsync += async (sender, ea) =>
        {
            byte[] body = ea.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"Your message sent: {message}");
            await Task.CompletedTask;
        };

        await channel.BasicConsumeAsync(queueName, true, consumerEvent, default).ConfigureAwait(false);
        Console.WriteLine("Your message sent");
        Console.ReadLine();
    }
}