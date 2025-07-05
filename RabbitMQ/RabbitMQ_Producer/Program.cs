    using RabbitMQ_Producer.DeadLetter;
using RabbitMQ_Producer.Default;
using RabbitMQ_Producer.Direct;
using RabbitMQ_Producer.ExchangeToExchange;
using RabbitMQ_Producer.Fanout;
using RabbitMQ_Producer.Header;
using RabbitMQ_Producer.LoadBalancing;
using RabbitMQ_Producer.RequestAndReply;
using RabbitMQ_Producer.Topic;

// Dictionary mapping menu options to corresponding producer actions
var producers = new Dictionary<string , Func<Task>>()
{
    ["1"] = () => new ProducerWithoutExchange().CallProducerAsync() ,
    ["2"] = () => new ProducerDirectExchange().CallProducerAsync() ,
    ["3"] = () => new ProducerTopicExchange().CallProducerAsync() ,
    ["4"] = () => new ProducerFanoutExchange().CallProducerAsync() ,
    ["5"] = () => new ProducerHeaderExchange().CallProducerAsync() ,
    ["6"] = () => new ProducerRequestAndReplyPattern().CallProducerAsync() ,
    ["7"] = () => new ProducerLoadBalancingExchange().CallProducerAsync() ,
    ["8"] = () => new ProducerExchangeToExchange().CallProducerAsync() ,
    ["9"] = () => new ProducerDeadLetterExchange().CallProducerAsync() ,
};

/// <summary>
/// Displays the menu options for the RabbitMQ producer application.
/// </summary>
void ShowMenu()
{
    Console.WriteLine("=== RabbitMQ Producer Menu ===");
    Console.WriteLine("1 - Default Queue");
    Console.WriteLine("2 - Direct Exchange");
    Console.WriteLine("3 - Topic Exchange");
    Console.WriteLine("4 - Fanout Exchange");
    Console.WriteLine("5 - Header Exchange");
    Console.WriteLine("6 - Request & Reply");
    Console.WriteLine("7 - Load Balancing");
    Console.WriteLine("8 - Exchange to Exchange");
    Console.WriteLine("9 - Dead Letter Exchange");
    Console.WriteLine("q - Quit");
}

string? input;
do
{
    // Show the menu and prompt the user for input
    ShowMenu();
    Console.Write("\nSelect an option: ");
    input = Console.ReadLine()?.Trim();

    // Exit if the user selects 'q'
    if ( string.Equals(input , "q" , StringComparison.OrdinalIgnoreCase) )
    {
        Console.WriteLine("Exiting...");
        break;
    }

    // Execute the selected producer action if valid
    if ( input != null && producers.TryGetValue(input , out var action) )
    {
        try
        {
            await action();
        }
        catch ( Exception ex )
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
    else
    {
        Console.WriteLine("Invalid option. Please try again.");
    }

} while ( true );
