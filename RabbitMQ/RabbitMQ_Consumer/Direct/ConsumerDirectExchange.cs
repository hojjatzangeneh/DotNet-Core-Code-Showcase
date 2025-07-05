using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System.Text;

namespace RabbitMQ_Consumer.Direct
{
    /// <summary>
    /// Consumes messages from a RabbitMQ direct exchange using a specific routing key.
    /// </summary>
    public class ConsumerDirectExchange
    {
        /// <summary>
        /// Connects to RabbitMQ, declares a direct exchange and a queue, binds them with a routing key,
        /// and listens for messages routed through the direct exchange.
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

            bool isAutoAckOn = true;

            // Establish connection and channel asynchronously
            using IConnection connection = await connectionFactory.CreateConnectionAsync().ConfigureAwait(false);
            using IChannel channel = await connection.CreateChannelAsync().ConfigureAwait(false);

            const string exchangeName = "DirectExchange";
            const string routingKey = "DirectRoutingKey";
            const string queueName = "DirectQueue";

            // Declare the direct exchange
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
            await channel.QueueBindAsync(
                queue: queueName ,
                exchange: exchangeName ,
                routingKey: routingKey ,
                arguments: null ,
                noWait: false ,
                cancellationToken: default)
                .ConfigureAwait(false);

            // Create an asynchronous consumer to handle incoming messages
            AsyncEventingBasicConsumer consumer = new(channel);

            consumer.ReceivedAsync += async (sender , ea) =>
            {
                try
                {
                    // Decode and display the received message
                    byte[] body = ea.Body.ToArray();
                    string message = Encoding.UTF8.GetString(body);
                    Console.WriteLine($"Received direct message: {message}");

                    // Manually acknowledge the message if auto-ack is off
                    if ( !isAutoAckOn )
                    {
                        await channel.BasicAckAsync(ea.DeliveryTag , false).ConfigureAwait(false);
                    }
                }
                catch ( Exception ex )
                {
                    // Log the error and reject the message
                    Console.WriteLine(ex.ToString());
                    await channel.BasicRejectAsync(ea.DeliveryTag , true).ConfigureAwait(false);
                }
                await Task.CompletedTask;
            };

            // Start consuming messages from the queue
            await channel.BasicConsumeAsync(
                queue: queueName ,
                autoAck: isAutoAckOn ,
                consumer: consumer)
                .ConfigureAwait(false);

            Console.WriteLine("Consumer is listening for direct messages...");
        }
    }
}