using RabbitMQ.Client;
using System;
using System.Linq;
using System.Text;

namespace RabbitMQ_Producer.Fanout
{
    /// <summary>
    /// Produces and sends a message to a RabbitMQ fanout exchange.
    /// </summary>
    public class ProducerFanoutExchange
    {
        /// <summary>
        /// Connects to RabbitMQ, declares a fanout exchange, and publishes a message to it.
        /// All queues bound to this exchange will receive the message.
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

            const string exchangeName = "FanoutExchange";
            const string routingKey = "";

            // Declare a fanout exchange (broadcasts messages to all bound queues)
            await channel.ExchangeDeclareAsync(
                exchange: exchangeName,
                type: ExchangeType.Fanout,
                durable: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: default)
                .ConfigureAwait(false);

            // Prepare the message body
            const string message = "This is a Fanout message from Producer";
            byte[] body = Encoding.UTF8.GetBytes(message);

            // Publish the message to the fanout exchange
            await channel.BasicPublishAsync(
                exchange: exchangeName,
                routingKey: routingKey,
                mandatory: false,
                body: body,
                cancellationToken: default)
                .ConfigureAwait(false);

            Console.WriteLine("Your message has been sent to the consumer.");
        }
    }
}