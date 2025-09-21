using KafkaApp.Models;

using Microsoft.Extensions.Options;

using System.Collections.Concurrent;

namespace KafkaApp.Kafka;

public sealed class MessageBuffer
{
    int _capacity;
    readonly ConcurrentQueue<ConsumedMessage> _queue = new();

    public MessageBuffer(IOptions<KafkaSettings> opt)
    {
        _capacity = Math.Max(1, opt.Value.ConsumeBufferCapacity);
    }

    public void Add(ConsumedMessage msg)
    {
        _queue.Enqueue(msg);
        while((_queue.Count > _capacity) && _queue.TryDequeue(out _))
        {
        }
    }

    public IReadOnlyList<ConsumedMessage> Snapshot(int take)
    {
        return _queue.Reverse().Take(Math.Max(1, take)).ToList();
    }
}