using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;

using System.Linq;
using System.Text;

namespace RabbitMQ_Consumer;

public class ConsumerExchange2ToExchange
{
    public async Task CallProducerAsync()
    {
        ConnectionFactory connectionFactory = new ConnectionFactory
        {
            HostName = "localhost"
        };
        const string firstExchange = "FirstExchange";

        const string queueName = "Exchange2ToExchangeQueue";
        await using IConnection connection = await connectionFactory.CreateConnectionAsync().ConfigureAwait(false);
        await using IChannel channel = await connection.CreateChannelAsync().ConfigureAwait(false);

        await channel.ExchangeDeclareAsync(firstExchange, type: ExchangeType.Direct).ConfigureAwait(false);
        await channel.QueueDeclareAsync(queueName, false, false, false, null, false, default)
            .ConfigureAwait(false);
        await channel.QueueBindAsync(queueName, firstExchange, queueName).ConfigureAwait(false);

        AsyncEventingBasicConsumer consumerEvent = new AsyncEventingBasicConsumer(channel);
        consumerEvent.ReceivedAsync += async (sender, ea) =>
        {
            byte[] body = ea.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"Recieved your message: {message}");
            await Task.CompletedTask;
        };

        await channel.BasicConsumeAsync(queueName, true, consumerEvent, default).ConfigureAwait(false);
        Console.WriteLine("Recieved your message");
        Console.ReadLine();
    }
}