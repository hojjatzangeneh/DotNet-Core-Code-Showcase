using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQ_Consumer;

internal class ConsumerWithoutExchange
{
    public async Task CallConsumerAsync()
    {
        // اتصال به RabbitMQ
        ConnectionFactory connectionFactory = new ConnectionFactory
        {
            HostName = "localhost"
        };

        using IConnection connection = await connectionFactory.CreateConnectionAsync().ConfigureAwait(false);
        using IChannel channel = await connection.CreateChannelAsync().ConfigureAwait(false);

        // تعریف صف (باید دقیقاً مطابق با Producer باشه)
        string queueName = "DirectMessage";
        await channel.QueueDeclareAsync(
            queue: queueName ,
            durable: false ,
            exclusive: false ,
            autoDelete: false ,
            arguments: null ,
            passive: false ,
            noWait: false ,
            cancellationToken: default
        )
            .ConfigureAwait(false);

        // تعریف مصرف کننده
        AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender , ea) =>
        {
            byte[] body = ea.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"Received message: {message}");

            // اگر autoAck = true باشه نیازی نیست این رو بنویسید، ولی برای درک بهتر قرار دادم
            await Task.CompletedTask;
        };

        // شروع مصرف
        await channel.BasicConsumeAsync(
            queue: queueName ,
            autoAck: true , // Auto Acknowledge
            consumer: consumer
        )
            .ConfigureAwait(false);

        Console.WriteLine("Consumer is listening for messages...");
        Console.ReadLine();
    }
}