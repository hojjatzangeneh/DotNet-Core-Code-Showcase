using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System.Text;

namespace RabbitMQ_Consumer.RequestAndReply
{
    /// <summary>
    /// Implements the consumer side of the Request-Reply pattern in RabbitMQ.
    /// Sends a request message and waits for a reply on a temporary exclusive queue.
    /// </summary>
    internal class ConsumerRequestAndReplyPattern
    {
        /// <summary>
        /// Connects to RabbitMQ, sends a request message with reply-to and correlation ID,
        /// and listens for the reply on a temporary queue.
        /// </summary>
        public async Task CallConsumerAsync()
        {
            // Create a connection factory with RabbitMQ server details
            var factory = new ConnectionFactory
            {
                HostName = "192.168.1.231" ,
                UserName = "guest" ,
                Password = "guest" ,
                Port = 5672
            };

            const string requestQueueName = "request-queue-pattern";

            // Establish connection and channel asynchronously
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            // Declare a temporary exclusive queue for receiving replies
            var replyQueue = await channel.QueueDeclareAsync("" , exclusive: true);

            // Create an asynchronous consumer for the reply queue
            var consumer = new AsyncEventingBasicConsumer(channel);

            string correlationId = "11";
            // TaskCompletionSource<string> tcs = new();

            // Handle received messages and check for matching correlation ID
            consumer.ReceivedAsync += (sender , ea) =>
            {
                if ( ea.BasicProperties.CorrelationId == correlationId )
                {
                    string response = Encoding.UTF8.GetString(ea.Body.ToArray());
                    Console.WriteLine($"[Consumer] Received reply: {response}");
                    // tcs.TrySetResult(response);
                }

                return Task.CompletedTask;
            };

            // Start consuming messages from the reply queue
            await channel.BasicConsumeAsync(
                queue: replyQueue.QueueName ,
                autoAck: true ,
                consumer: consumer);

            // Set up properties for the request message
            BasicProperties basicProperties = new()
            {
                ReplyTo = replyQueue.QueueName ,
                CorrelationId = correlationId
            };

            // Prepare the request message body
            string message = "Hello from Consumer!";
            byte[] body = Encoding.UTF8.GetBytes(message);

            // Publish the request message to the request queue
            await channel.BasicPublishAsync(
                exchange: "" ,
                routingKey: requestQueueName ,
                mandatory: false ,
                basicProperties: basicProperties ,
                body: body);

            Console.WriteLine("[Consumer] Request sent. Waiting for reply...");
            // await tcs.Task;

            Console.WriteLine("[Consumer] Done.");
        }
    }
}