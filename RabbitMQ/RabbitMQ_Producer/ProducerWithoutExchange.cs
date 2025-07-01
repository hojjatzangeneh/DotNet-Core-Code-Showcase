using RabbitMQ.Client;

using System.Text;

namespace RabbitMQ_Producer;

public class ProducerWithoutExchange
{
    public async Task CallProducerAsync()
    {
        // تعریف کانکشن اتصال
        ConnectionFactory connectionFactory = new ConnectionFactory
        {
            HostName = "localhost",
        };

        // اتصال و ایجاد کانال
        using IConnection connection = await connectionFactory.CreateConnectionAsync().ConfigureAwait(false);
        using IChannel channel = await connection.CreateChannelAsync().ConfigureAwait(false);

        // تعریف صف
        string queueName = "DefaultMessage";
        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            passive: false,
            noWait: false,
            cancellationToken: default)
            .ConfigureAwait(true);
        // آماده سازی پیام

        string message = "This is a message from Producer";
        byte[] body = Encoding.UTF8.GetBytes(message);

        // ارسال پیام

        await channel.BasicPublishAsync(
            exchange: string.Empty, // استفاده از exchange پیش فرض
            routingKey: queueName,// باید برابر با نام صف باشد
            mandatory: false,
            // basicProperties:null,
            body: body,
            cancellationToken: default)
            .ConfigureAwait(true);
        // تایید ارسال
        Console.WriteLine("Your message has send to consumer");
        Console.ReadLine();
    }
}