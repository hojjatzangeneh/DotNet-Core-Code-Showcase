using RabbitMQ_Consumer.DeadLetter;
using RabbitMQ_Consumer.Default;
using RabbitMQ_Consumer.Direct;
using RabbitMQ_Consumer.ExchangeToExchange;
using RabbitMQ_Consumer.Fanout;
using RabbitMQ_Consumer.Header;
using RabbitMQ_Consumer.LoadBalancing;
using RabbitMQ_Consumer.RequestAndReply;
using RabbitMQ_Consumer.Topic;

var Consumers = new Dictionary<string , (string Title, Func<Task> Action)>
{
    ["1"] = ("Default Queue", () =>
        new ConsumerWithoutExchange().CallConsumerAsync()) ,

    ["2"] = ("Direct Exchange", () =>
        new ConsumerDirectExchange().CallConsumerAsync()) ,

    ["3"] = ("Topic Exchange + Topic2 (Parallel)", () =>
        Task.WhenAll(
            new ConsumerTopicExchange().CallConsumerAsync() ,
            new ConsumerTopic2Exchange().CallConsumerAsync()
        )) ,

    ["4"] = ("Fanout Exchange + Fanout2 (Parallel)", () =>
        Task.WhenAll(
            new ConsumerFanoutExchange().CallConsumerAsync() ,
            new ConsumerFanout2Exchange().CallConsumerAsync()
        )) ,

    ["5"] = ("Header Exchange", () =>
        new ConsumerHeaderExchange().CallConsumerAsync()) ,

    ["6"] = ("Request & Reply", () =>
        new ConsumerRequestAndReplyPattern().CallConsumerAsync()) ,

    ["7"] = ("Load Balancing + LoadBalancing2 (Parallel)", () =>
        Task.WhenAll(
            new ConsumerLoadBalancingExchange().CallConsumerAsync() ,
            new ConsumerLoadBalancing2Exchange().CallConsumerAsync()
        )) ,

    ["8"] = ("Exchange to Exchange + Exchange2 (Parallel)", () =>
        Task.WhenAll(
            new ConsumerExchange2ToExchange().CallConsumerAsync() ,
            new ConsumerExchange2ToExchange().CallConsumerAsync()
        )) ,

    ["9"] = ("Dead Letter Exchange", () =>
        new ConsumerDeadLetterExchange().CallConsumerAsync()) ,
};

void ShowMenu()
{
    Console.Clear();
    Console.WriteLine("=== RabbitMQ Consumer Menu ===\n");
    foreach ( var item in Consumers )
    {
        Console.WriteLine($"{item.Key}. {item.Value.Title}");
    }
    Console.WriteLine("Q. Quit\n");
}

while ( true )
{
    ShowMenu();
    Console.Write("\nSelect an option: ");
    string? input = Console.ReadLine()?.Trim();

    if ( string.IsNullOrWhiteSpace(input) )
    {
        Console.WriteLine("\n⚠️ No input detected. Exiting...");
        break;
    }

    if ( string.Equals(input , "q" , StringComparison.OrdinalIgnoreCase) )
    {
        Console.WriteLine("\n👋 Exiting...");
        break;
    }

    if ( Consumers.TryGetValue(input , out var consumer) )
    {
        Console.WriteLine($"\n🔄 Running: {consumer.Title}\n");
        try
        {
            await consumer.Action();
            Console.WriteLine($"\n✅ Done: {consumer.Title}");
        }
        catch ( Exception ex )
        {
            Console.WriteLine($"\n❌ Failed: {ex.Message}");
        }
    }
    else
    {
        Console.WriteLine("\n⚠️ Invalid choice. Please try again.");
    }

    if ( !Console.IsInputRedirected )
    {
        Console.WriteLine("\n🔁 Press Enter to return to the menu...");
        Console.ReadLine();
    }
}
