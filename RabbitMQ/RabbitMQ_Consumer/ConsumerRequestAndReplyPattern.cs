using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQ_Consumer;

internal class ConsumerRequestAndReplyPattern
{
    public async Task CallConsumerAsync()
    {
        ConnectionFactory connectionFactory = new()
        {
            HostName = "localhost"
        };

        using IConnection connection = await connectionFactory.CreateConnectionAsync().ConfigureAwait(false);
        using IChannel channel = await connection.CreateChannelAsync().ConfigureAwait(false);
        const string queueName = "request-queue-pattern";
        QueueDeclareOk? replyQueue = await channel.QueueDeclareAsync("" , exclusive: true).ConfigureAwait(false);
        await channel.QueueDeclareAsync(queueName , exclusive: false).ConfigureAwait(false);
        // تعریف مصرف کننده
        AsyncEventingBasicConsumer consumerEvent = new(channel);

        consumerEvent.ReceivedAsync += async (sender , ea) =>
        {
            byte[] body = ea.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"Received message: {message}");

            // اگر autoAck = true باشه نیازی نیست این رو بنویسید، ولی برای درک بهتر قرار دادم
            await Task.CompletedTask;
        };

        // شروع مصرف
        await channel.BasicConsumeAsync(
            queue: replyQueue.QueueName ,
            autoAck: true , // Auto Acknowledge
            consumer: consumerEvent)
            .ConfigureAwait(false);

        BasicProperties basicProperties = new()
        {
            ReplyTo = replyQueue.QueueName ,
            CorrelationId = "11"
        };
        const string message = "This is a response message from Consumer";
        byte[] body = Encoding.UTF8.GetBytes(message);

        await channel.BasicPublishAsync(
            exchange: "" ,
            routingKey: queueName ,
            false ,
            basicProperties: basicProperties ,
            body: body ,
            default)
            .ConfigureAwait(false);
        Console.WriteLine("Consumer is listening for messages...");
        Console.ReadLine();
    }
}