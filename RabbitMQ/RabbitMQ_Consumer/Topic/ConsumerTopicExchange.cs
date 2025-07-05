using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Tasks;
using System;

namespace RabbitMQ_Consumer.Topic
{
    /// <summary>
    /// Consumes messages from a RabbitMQ topic exchange using a specific routing key pattern.
    /// </summary>
    public class ConsumerTopicExchange
    {
        /// <summary>
        /// Connects to RabbitMQ, declares a topic exchange and queue, binds them with a routing key pattern,
        /// and listens for messages matching the pattern.
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
            using var connection = await connectionFactory.CreateConnectionAsync().ConfigureAwait(false);
            using var channel = await connection.CreateChannelAsync().ConfigureAwait(false);

            const string exchangeName = "TopicExchange";
            const string queueName = "TopicQueue";
            const string routingKey = "hojjat.*.zangeneh";

            // Declare the topic exchange
            await channel.ExchangeDeclareAsync(exchange: exchangeName , type: ExchangeType.Topic).ConfigureAwait(false);

            // Declare the queue
            await channel.QueueDeclareAsync(
                queue: queueName ,
                durable: false ,
                exclusive: false ,
                autoDelete: false ,
                arguments: null).ConfigureAwait(false);

            // Bind the queue to the exchange with the routing key pattern
            await channel.QueueBindAsync(queue: queueName , exchange: exchangeName , routingKey: routingKey).ConfigureAwait(false);

            // Create an asynchronous consumer to handle incoming messages
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (sender , ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Received (Topic1): {message}");
                await Task.CompletedTask;
            };

            // Start consuming messages from the queue
            await channel.BasicConsumeAsync(queue: queueName , autoAck: true , consumer: consumer).ConfigureAwait(false);

            Console.WriteLine("ConsumerTopicExchange is listening for messages...");
            Console.WriteLine("Press [enter] to exit.");
        }
    }
}