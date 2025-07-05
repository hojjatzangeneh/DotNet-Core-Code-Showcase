using RabbitMQ.Client;
using System;
using System.Linq;
using System.Text;

namespace RabbitMQ_Producer.DeadLetter
{
    /// <summary>
    /// Produces and sends a message to a queue configured with a dead-letter exchange in RabbitMQ.
    /// Demonstrates how to set up dead-lettering for messages that expire or are rejected.
    /// </summary>
    public class ProducerDeadLetterExchange
    {
        /// <summary>
        /// Connects to RabbitMQ, declares a dead-letter exchange and queue, configures the main queue
        /// with dead-letter arguments, and publishes a message to the main queue.
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
            await using IConnection connection = await connectionFactory.CreateConnectionAsync().ConfigureAwait(false);
            await using IChannel channel = await connection.CreateChannelAsync().ConfigureAwait(false);

            const string deadLetterExchangeName = "DeadLetterExchange";
            const string deadLetterQueueName = "DeadLetterQueue";
            const string queueName = "MainQueue";
            const string routingKey = "errorMessage";

            // Declare the dead-letter exchange (direct type)
            await channel.ExchangeDeclareAsync(deadLetterExchangeName, type: ExchangeType.Direct).ConfigureAwait(false);

            // Declare the dead-letter queue
            await channel.QueueDeclareAsync(
                queue: deadLetterQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null)
                .ConfigureAwait(true);

            // Bind the dead-letter queue to the dead-letter exchange with the routing key
            await channel.QueueBindAsync(deadLetterQueueName, deadLetterExchangeName, routingKey).ConfigureAwait(false);

            // Set up arguments for the main queue to enable dead-lettering and message TTL
            Dictionary<string, object?> queueArguments = new Dictionary<string, object?>
            {
                { "x-dead-letter-exchange", deadLetterExchangeName },
                { "x-dead-letter-routing-key", routingKey },
                { "x-message-ttl", 10000 }
            };

            // Declare the main queue with dead-letter arguments
            await channel.QueueDeclareAsync(queueName, true, false, false, arguments: queueArguments)
                .ConfigureAwait(false);

            // Prepare the message body
            const string message = "This is a Fanout DeadLetter from Producer";
            byte[] body = Encoding.UTF8.GetBytes(message);

            // Publish the message to the main queue (using the default exchange)
            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queueName,
                mandatory: false,
                body: body,
                cancellationToken: default)
                .ConfigureAwait(false);

            Console.WriteLine("Your message has been sent to the consumer.");
            // Wait for user input to keep the application running
            Console.ReadLine();
        }
    }
}