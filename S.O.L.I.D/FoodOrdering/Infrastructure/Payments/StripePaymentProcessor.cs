using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FoodOrdering.Application.Payments;

namespace FoodOrdering.Infrastructure.Payments;
// OCP: اضافه شدن کلاس جدید بدون تغییر در OrderService
public class StripePaymentProcessor : IPaymentProcessor
{
    public async Task<PaymentResult> ProcessPaymentAsync(decimal amount , PaymentDetails details)
    {
        // شبیه سازی تماس به Stripe
        await Task.Delay(50);
        return new PaymentResult(true , Guid.NewGuid().ToString());
    }
}
