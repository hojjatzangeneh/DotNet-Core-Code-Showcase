using Confluent.Kafka;


using System.Text;

namespace KafkaApp.Models;

public sealed class ProduceRequest
{
    public Dictionary<string, string>? Headers { get; set; }

    public string? Key { get; set; }

    public string? Topic { get; set; }

    public object? Value { get; set; }
}

public static class HeaderExtensions
{
    public static Headers? ToHeaders(this Dictionary<string, string>? dict)
    {
        if((dict is null) || (dict.Count == 0))
        {
            return null;
        }

        Headers hs = new Headers();
        foreach(KeyValuePair<string, string> kv in dict)
        {
            hs.Add(kv.Key, Encoding.UTF8.GetBytes(kv.Value ?? string.Empty));
        }

        return hs;
    }
}