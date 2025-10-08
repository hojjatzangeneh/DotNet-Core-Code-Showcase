using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FoodOrdering.Application.Notifications;

namespace FoodOrdering.Infrastructure.Notifications;
public class EmailNotificationService : INotificationService
{
    public Task SendOrderConfirmationAsync(string email , Guid orderId)
    {
        // SRP: این کلاس فقط مسئول ارسال ایمیل است
        Console.WriteLine($"(Email) To: {email} - Order {orderId} received");
        return Task.CompletedTask;
    }
}
