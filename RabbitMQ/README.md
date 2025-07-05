# RabbitMQ Producer & Consumer (.NET 9)

This folder contains two simple .NET 9 projects demonstrating basic usage of RabbitMQ without using an exchange. The projects are:

- **RabbitMQ_Producer**: Sends messages to a queue.
- **RabbitMQ_Consumer**: Receives messages from the same queue.

## Prerequisites

- .NET 9 SDK
- RabbitMQ server (default connection: `192.168.1.231:5672`, user: `guest`, password: `guest`)
- (Optional) Docker, if you want to run the producer in a container

## Queue Details

Both projects use a queue named `DefaultMessage`. No custom exchange is used; the default exchange is applied.

---

## RabbitMQ_Producer

Sends a simple text message to the `DefaultMessage` queue.

### Run Locally

cd RabbitMQ/RabbitMQ_Producer dotnet run

### Run with Docker

Build and run the Docker image:

docker build -t rabbitmq_producer . docker run --rm rabbitmq_producer


---

## Notes

- Make sure the RabbitMQ server is running and accessible at the configured address.
- The queue must match between producer and consumer (`DefaultMessage`).
- Both projects use auto-acknowledge for message consumption.

---

## Project Structure

- `RabbitMQ_Producer/ProducerWithoutExchange.cs`: Producer logic
- `RabbitMQ_Consumer/ConsumerWithoutExchange.cs`: Consumer logic
- `RabbitMQ_Producer/Dockerfile`: Docker support for the producer

---

Feel free to modify the connection settings or queue name as needed.


