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

    /// <summary>
    /// افزودن پیام جدید؛ ظرفیت را حفظ می‌کند.
    /// </summary>
    public void Add(ConsumedMessage msg)
    {
        _queue.Enqueue(msg);
        // اگر از ظرفیت گذشت، از سر صف حذف کن
        while((_queue.Count > _capacity) && _queue.TryDequeue(out _))
        {
        }
    }

    /// <summary>
    /// پاک کردن n پیام (یا همه اگر take=null). خروجی: تعداد حذف‌شده‌ها.
    /// </summary>
    public int Clear(int? take = null)
    {
        int removed = 0;
        int limit = take ?? int.MaxValue;
        while((removed < limit) && _queue.TryDequeue(out _))
        {
            removed++;
        }

        return removed;
    }

    /// <summary>
    /// برداشت و حذف n پیام از صف (خواندن به‌همراه حذف).
    /// </summary>
    public IReadOnlyList<ConsumedMessage> Drain(int take)
    {
        int n = Math.Max(1, take);
        List<ConsumedMessage> list = new List<ConsumedMessage>(n);
        for(int i = 0; (i < n) && _queue.TryDequeue(out ConsumedMessage? msg); i++)
        {
            list.Add(msg);
        }

        return list;
    }

    /// <summary>
    /// تغییر ظرفیت در زمان اجرا (اختیاری).
    /// </summary>
    public void SetCapacity(int capacity)
    {
        _capacity = Math.Max(1, capacity);
        // اگر صف از ظرفیت جدید بزرگ‌تر است، کمش کن
        while((_queue.Count > _capacity) && _queue.TryDequeue(out _))
        {
        }
    }

    /// <summary>
    /// فقط دیدن پیام‌ها (بدون حذف). آخرین پیام‌ها اول می‌آیند.
    /// </summary>
    public IReadOnlyList<ConsumedMessage> Snapshot(int take)
    {
        return _queue.Reverse().Take(Math.Max(1, take)).ToList();
    }
}
