using RabbitMQ_Consumer;

var consumerWithoutExchange = new ConsumerWithoutExchange();
await consumerWithoutExchange.CallConsumerAsync();
