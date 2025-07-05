using RabbitMQ.Client;
using System.Text;

namespace RabbitMQ_Producer.Default
{
    /// <summary>
    /// Produces and sends a message directly to a RabbitMQ queue without using an exchange.
    /// </summary>
    public class ProducerWithoutExchange
    {
        /// <summary>
        /// Connects to RabbitMQ, declares a queue, and publishes a message directly to it using the default exchange.
        /// </summary>
        public async Task CallProducerAsync()
        {
            // Create a connection factory with RabbitMQ server details
            ConnectionFactory connectionFactory = new ConnectionFactory
            {
                HostName = "192.168.1.231",
                UserName = "guest",
                Password = "guest",
                Port = 5672
            };

            // Establish connection and channel asynchronously
            using IConnection connection = await connectionFactory.CreateConnectionAsync().ConfigureAwait(false);
            using IChannel channel = await connection.CreateChannelAsync().ConfigureAwait(false);

            const string queueName = "DefaultQueue";

            // Declare the queue to ensure it exists
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

            // Prepare the message body
            string message = "This is a message from Producer";
            byte[] body = Encoding.UTF8.GetBytes(message);

            // Publish the message to the default exchange (empty string) with the queue name as the routing key
            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queueName,
                mandatory: false,
                body: body,
                cancellationToken: default)
                .ConfigureAwait(true);

            Console.WriteLine("Your message has send to consumer");
        }
    }
}