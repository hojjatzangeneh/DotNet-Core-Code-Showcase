using RabbitMQ.Client;
using System;
using System.Linq;
using System.Text;

namespace RabbitMQ_Producer.ExchangeToExchange
{
    /// <summary>
    /// Produces and sends a message to a direct exchange, which is then routed to a fanout exchange using exchange-to-exchange binding.
    /// </summary>
    public class ProducerExchangeToExchange
    {
        /// <summary>
        /// Connects to RabbitMQ, declares two exchanges (direct and fanout), binds them together,
        /// and publishes a message to the first exchange so it is routed to the second exchange.
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

            const string firstExchange = "FirstExchangeToExchange";
            const string secondExchange = "SecondExchangeToExchange";
            const string routingKey = "ExchangeTo2Exchange"; // Routing key for binding

            // Establish connection and channel asynchronously
            await using IConnection connection = await connectionFactory.CreateConnectionAsync().ConfigureAwait(false);
            await using IChannel channel = await connection.CreateChannelAsync().ConfigureAwait(false);

            // Declare the first exchange (direct type)
            await channel.ExchangeDeclareAsync(firstExchange, type: ExchangeType.Direct).ConfigureAwait(false);

            // Declare the second exchange (fanout type)
            await channel.ExchangeDeclareAsync(secondExchange, type: ExchangeType.Fanout).ConfigureAwait(false);

            // Bind the second exchange to the first exchange with the routing key
            await channel.ExchangeBindAsync(secondExchange, firstExchange, routingKey).ConfigureAwait(false);

            // Prepare the message body
            const string message = "This is a ExchangeToExchange message from Producer";
            byte[] body = Encoding.UTF8.GetBytes(message);

            // Publish the message to the first exchange
            await channel.BasicPublishAsync(
                exchange: firstExchange,
                routingKey: routingKey,
                mandatory: false,
                body: body,
                cancellationToken: default)
                .ConfigureAwait(false);

            Console.WriteLine("Your message sent");
        }
    }
}