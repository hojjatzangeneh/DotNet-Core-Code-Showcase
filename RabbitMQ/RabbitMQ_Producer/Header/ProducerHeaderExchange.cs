using RabbitMQ.Client;
using System;
using System.Linq;
using System.Text;

namespace RabbitMQ_Producer.Header
{
    /// <summary>
    /// Produces and sends a message to a RabbitMQ headers exchange with custom headers.
    /// </summary>
    public class ProducerHeaderExchange
    {
        /// <summary>
        /// Connects to RabbitMQ, declares a headers exchange, and publishes a message with specific headers.
        /// </summary>
        public async Task CallProducerAsync()
        {
            // Create a connection factory with RabbitMQ server details
            ConnectionFactory connectionFactory = new()
            {
                HostName = "192.168.1.231",
                UserName = "guest",
                Password = "guest",
                Port = 5672
            };

            // Establish connection and channel asynchronously
            await using IConnection connection = await connectionFactory.CreateConnectionAsync().ConfigureAwait(false);
            await using IChannel channel = await connection.CreateChannelAsync().ConfigureAwait(false);

            const string exchangeName = "HeaderExchange";
            const string routingKey = "";

            // Declare a durable headers exchange
            await channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Headers, durable: true).ConfigureAwait(false);

            // Set up message properties with custom headers
            BasicProperties basicProperties = new()
            {
                ContentType = "text/plain",
                DeliveryMode = DeliveryModes.Persistent,
                Headers = new Dictionary<string, object?>
                {
                    { "format", "pdf" },
                    { "type", "report" },
                    //{ "x-match", "all" } // Uncomment to require all headers to match
                }
            };

            // Prepare the message body
            const string message = "This is a header message from Producer";
            byte[] body = Encoding.UTF8.GetBytes(message);

            // Publish the message to the headers exchange
            await channel.BasicPublishAsync(
                exchange: exchangeName,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: basicProperties,
                body: body
            ).ConfigureAwait(false);

            Console.WriteLine("Your message has been sent to the consumer.");
            await Task.CompletedTask;
        }
    }
}