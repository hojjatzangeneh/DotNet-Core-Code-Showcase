using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System.Text;

namespace RabbitMQ_Consumer.Header
{
    /// <summary>
    /// Consumes messages from a RabbitMQ headers exchange using specific header matching.
    /// </summary>
    public class ConsumerHeaderExchange
    {
        /// <summary>
        /// Connects to RabbitMQ, declares a headers exchange and queue, binds them with header arguments,
        /// and listens for messages that match the specified headers.
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

            // Define header arguments for binding (messages must match all headers)
            Dictionary<string , object?>? arguments = new()
            {
                { "format", "pdf" },
                { "type", "report" },
                { "x-match", "all" }
            };

            const string exchangeName = "HeaderExchange";
            const string routingKey = "";
            const string queueName = "HeaderQueue";

            // Declare the headers exchange
            await channel.ExchangeDeclareAsync(
                exchange: exchangeName ,
                type: ExchangeType.Headers ,
                durable: true ,
                autoDelete: false ,
                arguments: null ,
                cancellationToken: default)
                .ConfigureAwait(false);

            // Declare the queue
            await channel.QueueDeclareAsync(
                queue: queueName ,
                durable: true ,
                exclusive: false ,
                autoDelete: false ,
                arguments: null ,
                cancellationToken: default)
                .ConfigureAwait(false);

            // Bind the queue to the exchange with the specified header arguments
            await channel.QueueBindAsync(
                queue: queueName ,
                exchange: exchangeName ,
                routingKey: routingKey ,
                arguments: arguments ,
                noWait: false ,
                cancellationToken: default)
                .ConfigureAwait(false);

            // Create an asynchronous consumer to handle incoming messages
            AsyncEventingBasicConsumer consumer = new(channel);

            consumer.ReceivedAsync += async (sender , ea) =>
            {
                // Decode and display the received message
                byte[] body = ea.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Received Header message: {message}");
                await Task.CompletedTask;
            };

            // Start consuming messages from the queue
            await channel.BasicConsumeAsync(
                queue: queueName ,
                autoAck: true ,
                consumer: consumer)
                .ConfigureAwait(false);

            Console.WriteLine("Consumer is listening for Header messages...");
        }
    }
}