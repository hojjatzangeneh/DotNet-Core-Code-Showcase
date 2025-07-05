using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;
using System.Linq;
using System.Text;

namespace RabbitMQ_Consumer.LoadBalancing
{
    /// <summary>
    /// Consumes messages from a RabbitMQ queue to demonstrate load balancing between multiple consumers.
    /// This class represents a second consumer instance for the same queue.
    /// </summary>
    public class ConsumerLoadBalancing2Exchange
    {
        /// <summary>
        /// Connects to RabbitMQ, declares a queue, and listens for messages.
        /// Each consumer instance will compete for messages, enabling load balancing.
        /// </summary>
        public async Task CallConsumerAsync()
        {
            // Create a connection factory with RabbitMQ server details
            ConnectionFactory connectionFactory = new ConnectionFactory
            {
                HostName = "192.168.1.231" ,
                UserName = "guest" ,
                Password = "guest" ,
                Port = 5672
            };

            // Establish connection and channel asynchronously
            await using IConnection connection = await connectionFactory.CreateConnectionAsync().ConfigureAwait(false);
            await using IChannel channel = await connection.CreateChannelAsync().ConfigureAwait(false);

            const string queueName = "LoadBalancingQueue";

            // Declare the queue for load balancing
            await channel.QueueDeclareAsync(
                queue: queueName ,
                durable: false ,
                exclusive: false ,
                arguments: null ,
                noWait: false ,
                cancellationToken: default)
                .ConfigureAwait(false);

            // Create an asynchronous consumer to handle incoming messages
            AsyncEventingBasicConsumer consumerEvent = new(channel);

            consumerEvent.ReceivedAsync += async (sender , ea) =>
            {
                // Decode and display the received message
                byte[] body = ea.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Received direct message: {message}");
                await Task.CompletedTask;
            };

            // Start consuming messages from the queue
            await channel.BasicConsumeAsync(
                queue: queueName ,
                autoAck: true ,
                consumer: consumerEvent)
                .ConfigureAwait(false);

            Console.WriteLine("Consumer is listening for LoadBalancing messages...");
        }
    }
}