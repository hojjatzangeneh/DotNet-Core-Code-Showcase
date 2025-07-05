using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System.Text;

namespace RabbitMQ_Consumer.ExchangeToExchange;

public class ConsumerExchangeToExchange
{
    public async Task CallConsumerAsync()
    {
        var connectionFactory = new ConnectionFactory
        {
            HostName = "192.168.1.231" ,
            UserName = "guest" ,
            Password = "guest" ,
            Port = 5672
        };

        const string firstExchange = "FirstExchangeToExchange";
        const string queueName = "Exchange2ToExchangeQueue";
        const string routingKey = "ExchangeToExchange";

        await using var connection = await connectionFactory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(firstExchange , ExchangeType.Direct);
        await channel.QueueDeclareAsync(queue: queueName , durable: false , exclusive: false , autoDelete: false);
        await channel.QueueBindAsync(queue: queueName , exchange: firstExchange , routingKey: routingKey);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (sender , ea) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            Console.WriteLine($"[Consumer2] Message received: {message}");
            await Task.CompletedTask;
        };

        await channel.BasicConsumeAsync(queue: queueName , autoAck: true , consumer: consumer);
        Console.WriteLine("[Consumer2] Waiting for messages...");
    }
}
