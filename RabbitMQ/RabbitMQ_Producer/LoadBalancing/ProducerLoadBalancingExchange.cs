using RabbitMQ.Client;
using System;
using System.Linq;
using System.Text;

namespace RabbitMQ_Producer.LoadBalancing
{
    /// <summary>
    /// Produces and sends a series of messages to a single queue in RabbitMQ to demonstrate load balancing.
    /// </summary>
    public class ProducerLoadBalancingExchange
    {
        /// <summary>
        /// Connects to RabbitMQ, declares a queue, and publishes multiple messages to it with a delay between each message.
        /// This simulates a load balancing scenario where multiple consumers can process messages from the same queue.
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

            const string queueName = "LoadBalancingQueue";

            // Declare the queue for load balancing
            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: false,
                exclusive: false,
                arguments: null,
                noWait: false,
                cancellationToken: default)
                .ConfigureAwait(false);

            // Publish 11 messages to the queue, with a delay between each
            for (int i = 0; i < 11; i++)
            {
                string message = $"Message ==> {i}";
                byte[] body = Encoding.UTF8.GetBytes(message);

                // Send the message to the queue
                await channel.BasicPublishAsync(string.Empty, queueName, body, default).ConfigureAwait(false);

                // Wait for 1.5 seconds before sending the next message
                Thread.Sleep(1500);
            }
        }
    }
}