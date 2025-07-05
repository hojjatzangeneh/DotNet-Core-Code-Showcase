using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;
using System.Linq;
using System.Text;

namespace RabbitMQ_Consumer.DeadLetter
{
    /// <summary>
    /// Consumes messages from a RabbitMQ queue configured with dead-lettering.
    /// Demonstrates how to handle messages that are rejected or expired and routed to a dead-letter exchange.
    /// </summary>
    public class ConsumerDeadLetterExchange
    {
        /// <summary>
        /// Connects to RabbitMQ, declares a queue with dead-letter arguments, and listens for messages.
        /// Each received message is rejected to trigger dead-lettering.
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

            const string deadLetterExchangeName = "DeadLetterExchange";
            const string routingKey = "errorMessage";
            const string queueName = "MainQueue";

            // Set up arguments for the main queue to enable dead-lettering and message TTL
            Dictionary<string , object?> arguments = new()
            {
                { "x-dead-letter-exchange", deadLetterExchangeName },
                { "x-dead-letter-routing-key", routingKey },
                { "x-message-ttl", 10000 }
            };

            // Declare the main queue with dead-letter arguments
            await channel.QueueDeclareAsync(queueName , true , false , false , arguments: arguments)
                .ConfigureAwait(false);

            // Create an asynchronous consumer to handle incoming messages
            AsyncEventingBasicConsumer consumerEvent = new(channel);
            consumerEvent.ReceivedAsync += async (sender , ea) =>
            {
                // Decode and display the received message
                byte[] body = ea.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Received DeadLetter message: {message}");

                // Reject the message to trigger dead-lettering
                await channel.BasicRejectAsync(ea.DeliveryTag , false).ConfigureAwait(false);
                await Task.CompletedTask;
            };

            // Start consuming messages from the queue (manual acknowledgment)
            await channel.BasicConsumeAsync(
                queue: queueName ,
                autoAck: false ,
                consumer: consumerEvent)
                .ConfigureAwait(false);

            Console.WriteLine("Consumer is listening for LoadBalancing messages...");
        }
    }
}