using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;

using System.Linq;
using System.Text;

namespace RabbitMQ_Producer;

public class ProducerRequestAndReplyPattern
{
    public async Task CallProducerAsync()
    {
        ConnectionFactory connectionFactory = new ConnectionFactory
        {
            HostName = "localhost"
        };
        const string queueName = "request-queue-pattern";

        await using IConnection connection = await connectionFactory.CreateConnectionAsync().ConfigureAwait(false);
        await using IChannel channel = await connection.CreateChannelAsync().ConfigureAwait(false);
        await channel.QueueDeclareAsync(queueName, exclusive: false).ConfigureAwait(false);
        AsyncEventingBasicConsumer? consumerEvent = new AsyncEventingBasicConsumer(channel);
        consumerEvent.ReceivedAsync += async (sender, ea) =>
        {
            Console.WriteLine($"CorrelationId : {ea.BasicProperties.CorrelationId}");
            Console.WriteLine($"ReplyTo : {ea.BasicProperties.ReplyTo}");
            string message = $"this message is response fro {ea.BasicProperties.CorrelationId}";
            byte[] body = Encoding.UTF8.GetBytes(message);
            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: ea.BasicProperties.ReplyTo ?? string.Empty,
                mandatory: false,
                // basicProperties: null ,
                body: body,
                cancellationToken: default);
        };
        await channel.BasicConsumeAsync(
            queueName,
            autoAck: true,
            consumerTag: string.Empty,
            noLocal: false,
            exclusive: true,
            arguments: null,
            consumer: consumerEvent)
            .ConfigureAwait(false);

        Console.ReadLine();
    }
}