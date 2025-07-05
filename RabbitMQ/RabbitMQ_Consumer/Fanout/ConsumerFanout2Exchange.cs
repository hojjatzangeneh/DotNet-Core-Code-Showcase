using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System.Text;

namespace RabbitMQ_Consumer.Fanout
{
    /// <summary>
    /// Consumes messages from a RabbitMQ fanout exchange.
    /// This class represents a second consumer instance for the same fanout exchange.
    /// </summary>
    public class ConsumerFanout2Exchange
    {
        /// <summary>
        /// Connects to RabbitMQ, declares a fanout exchange and a queue, binds them,
        /// and listens for messages broadcasted to the exchange.
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

            const string exchangeName = "FanoutExchange";
            const string routingKey = "";
            const string queueName = "Fanout2Queue";

            // Declare the fanout exchange (broadcasts messages to all bound queues)
            await channel.ExchangeDeclareAsync(
                exchange: exchangeName ,
                type: ExchangeType.Fanout ,
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
                cancellationToken: default)
                .ConfigureAwait(false);

            // Bind the queue to the fanout exchange
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
                // Decode and display the received message
                byte[] body = ea.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Received fanout message: {message}");
                await Task.CompletedTask;
            };

            // Start consuming messages from the queue
            await channel.BasicConsumeAsync(
                queue: queueName ,
                autoAck: true ,
                consumer: consumer)
                .ConfigureAwait(false);

            Console.WriteLine("Consumer is listening for fanout messages...");
        }
    }
}