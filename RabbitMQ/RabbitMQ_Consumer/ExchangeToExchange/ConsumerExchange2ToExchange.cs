using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System.Text;

namespace RabbitMQ_Consumer.ExchangeToExchange
{
    /// <summary>
    /// Consumes messages from a RabbitMQ fanout exchange as part of an exchange-to-exchange binding scenario.
    /// </summary>
    public class ConsumerExchange2ToExchange
    {
        /// <summary>
        /// Connects to RabbitMQ, declares a fanout exchange and a queue, binds them,
        /// and listens for messages routed through the exchange-to-exchange setup.
        /// </summary>
        public async Task CallConsumerAsync()
        {
            // Create a connection factory with RabbitMQ server details
            var connectionFactory = new ConnectionFactory
            {
                HostName = "192.168.1.231" ,
                UserName = "guest" ,
                Password = "guest" ,
                Port = 5672
            };

            const string secondExchange = "SecondExchangeToExchange";
            const string queueName = "ExchangeToExchangeQueue";

            // Establish connection and channel asynchronously
            await using var connection = await connectionFactory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            // Declare the fanout exchange
            await channel.ExchangeDeclareAsync(secondExchange , ExchangeType.Fanout);

            // Declare the queue
            await channel.QueueDeclareAsync(queue: queueName , durable: false , exclusive: false , autoDelete: false);

            // Bind the queue to the fanout exchange
            await channel.QueueBindAsync(queue: queueName , exchange: secondExchange , routingKey: "");

            // Create an asynchronous consumer to handle incoming messages
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (sender , ea) =>
            {
                // Decode and display the received message
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                Console.WriteLine($"[Consumer1] Message received: {message}");
                await Task.CompletedTask;
            };

            // Start consuming messages from the queue
            await channel.BasicConsumeAsync(queue: queueName , autoAck: true , consumer: consumer);
            Console.WriteLine("[Consumer1] Waiting for messages...");
        }
    }
}