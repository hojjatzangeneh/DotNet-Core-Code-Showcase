using RabbitMQ.Client;

using System.Text;

namespace RabbitMQ_Producer.Direct
{
    /// <summary>
    /// Produces and sends a message to a RabbitMQ direct exchange with a specific routing key.
    /// </summary>
    public class ProducerDirectExchange
    {
        /// <summary>
        /// Connects to RabbitMQ, declares a direct exchange and a queue, binds them with a routing key,
        /// and publishes a message to the exchange so it is routed to the bound queue.
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
            await using IConnection connection = await connectionFactory.CreateConnectionAsync().ConfigureAwait(false);
            await using IChannel channel = await connection.CreateChannelAsync().ConfigureAwait(false);

            const string exchangeName = "DirectExchange";
            const string routingKey = "DirectRoutingKey";
            const string queueName = "DirectQueue";

            // Declare a direct exchange
            await channel.ExchangeDeclareAsync(
                exchange: exchangeName ,
                type: ExchangeType.Direct ,
                durable: false ,
                autoDelete: false ,
                arguments: null ,
                cancellationToken: default)
                .ConfigureAwait(false);

            // Declare the queue
            await channel.QueueDeclareAsync(
                queue: queueName ,
                durable: false ,
                exclusive: false ,
                autoDelete: false ,
                arguments: null ,
                cancellationToken: default
            ).ConfigureAwait(false);

            // Bind the queue to the exchange with the routing key
            await channel.QueueBindAsync(queueName , exchangeName , routingKey).ConfigureAwait(false);

            // Prepare the message body
            const string message = "This is a direct message from Producer";
            byte[] body = Encoding.UTF8.GetBytes(message);

            // Publish the message to the direct exchange
            await channel.BasicPublishAsync(
                exchange: exchangeName ,
                routingKey: routingKey ,
                mandatory: false ,
                //  basicProperties: null ,
                body: body ,
                cancellationToken: default
            ).ConfigureAwait(false);

            Console.WriteLine($"[x] Sent: '{message}' to exchange '{exchangeName}' with routing key '{routingKey}'");
        }
    }
}