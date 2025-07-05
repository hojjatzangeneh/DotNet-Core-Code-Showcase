using System.Text;

using RabbitMQ.Client;

namespace RabbitMQ_Producer.Topic
{
    /// <summary>
    /// Produces and sends messages to a RabbitMQ topic exchange using various routing keys.
    /// </summary>
    public class ProducerTopicExchange
    {
        /// <summary>
        /// Connects to RabbitMQ, declares a topic exchange, and publishes messages with different routing keys.
        /// </summary>
        public async Task CallProducerAsync()
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
            await using var connection = await connectionFactory.CreateConnectionAsync().ConfigureAwait(false);
            await using var channel = await connection.CreateChannelAsync().ConfigureAwait(false);

            const string exchangeName = "TopicExchange";

            // Declare a topic exchange
            await channel.ExchangeDeclareAsync(exchange: exchangeName , type: ExchangeType.Topic).ConfigureAwait(false);

            // Define routing keys for publishing messages
            string[] routingKeys = {
                "hojjat.one.zangeneh",
                "hojjat.two.zangeneh",
                "hojjat.one.two.zangeneh",
                "other.topic.value"
            };

            // Publish a message for each routing key
            foreach ( var routingKey in routingKeys )
            {
                string message = $"Message with routing key: {routingKey}";
                byte[] body = Encoding.UTF8.GetBytes(message);

                await channel.BasicPublishAsync(
                    exchange: exchangeName ,
                    routingKey: routingKey ,
                    body: body ,
                    cancellationToken: default).ConfigureAwait(false);

                // Log the sent message
                Console.WriteLine($"✅ Sent: {message}");
            }
        }
    }
}