using RabbitMQ_Producer;

ProducerWithoutExchange producerWithoutExchange = new ProducerWithoutExchange();
await producerWithoutExchange.CallProducerAsync();
