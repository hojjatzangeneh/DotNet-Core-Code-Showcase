using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodOrdering.Application.Notifications;
public interface INotificationService
{
    Task SendOrderConfirmationAsync(string email , Guid orderId);
}
