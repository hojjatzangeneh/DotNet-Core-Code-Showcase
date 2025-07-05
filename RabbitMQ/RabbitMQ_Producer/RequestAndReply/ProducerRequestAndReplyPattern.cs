using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RabbitMQ_Producer.RequestAndReply
{
    /// <summary>
    /// Implements the Request-Reply pattern for RabbitMQ producer.
    /// Listens for requests on a queue and sends replies to the specified reply queue.
    /// </summary>
    public class ProducerRequestAndReplyPattern
    {
        /// <summary>
        /// Connects to RabbitMQ, consumes requests from a queue, and sends replies to the reply queue specified in the request properties.
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

            const string queueName = "request-queue-pattern";

            // Establish connection and channel asynchronously
            await using var connection = await connectionFactory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            // Declare the request queue
            await channel.QueueDeclareAsync(queue: queueName, durable: false, exclusive: false, autoDelete: false);

            // Create an asynchronous consumer for handling incoming requests
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (sender, ea) =>
            {
                // Log the received request and its correlation ID
                Console.WriteLine($"[Producer] Received request: {Encoding.UTF8.GetString(ea.Body.ToArray())}");
                Console.WriteLine($"[Producer] CorrelationId: {ea.BasicProperties.CorrelationId}");

                // Prepare the response message
                string response = $"Reply from Producer to {ea.BasicProperties.CorrelationId}";
                byte[] body = Encoding.UTF8.GetBytes(response);

                // Set up properties for the reply message
                BasicProperties basicProperties = new()
                {
                    ReplyTo = ea.BasicProperties.ReplyTo ?? string.Empty,
                    CorrelationId = "11" // You may want to use the original CorrelationId here
                };

                // Publish the reply message to the reply queue
                await channel.BasicPublishAsync(
                    exchange: "",
                    routingKey: ea.BasicProperties.ReplyTo ?? string.Empty,
                    mandatory: false,
                    basicProperties: basicProperties,
                    body: body);

                Console.WriteLine("[Producer] Response sent.\n");
            };

            // Start consuming requests from the queue
            await channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: true,
                consumer: consumer);

            Console.WriteLine("[Producer] Waiting for requests...");
            // Keep the application running to listen for requests
            Console.ReadLine();
        }
    }
}
    