using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQ_Consumer.Default
{
    /// <summary>
    /// Consumes messages directly from a RabbitMQ queue without using a custom exchange.
    /// </summary>
    internal class ConsumerWithoutExchange
    {
        /// <summary>
        /// Connects to RabbitMQ, declares a queue, and listens for messages sent directly to the queue.
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
            using IConnection connection = await connectionFactory.CreateConnectionAsync().ConfigureAwait(false);
            using IChannel channel = await connection.CreateChannelAsync().ConfigureAwait(false);

            const string queueName = "DefaultQueue";

            // Declare the queue to ensure it exists
            await channel.QueueDeclareAsync(
                queue: queueName ,
                durable: false ,
                exclusive: false ,
                autoDelete: false ,
                arguments: null ,
                passive: false ,
                noWait: false ,
                cancellationToken: default
            ).ConfigureAwait(false);

            // Create an asynchronous consumer to handle incoming messages
            AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (sender , ea) =>
            {
                // Decode and display the received message
                byte[] body = ea.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Received message: {message}");

                await Task.CompletedTask;
            };

            // Start consuming messages from the queue
            await channel.BasicConsumeAsync(
                queue: queueName ,
                autoAck: true ,
                consumer: consumer
            ).ConfigureAwait(false);

            Console.WriteLine("Consumer is listening for messages...");
        }
    }
}