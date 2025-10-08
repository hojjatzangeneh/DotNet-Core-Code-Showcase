using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodOrdering.Application.Payments;
// DIP: وابستگی به abstraction. OCP: اضافه کردن روش های پرداخت جدید بدون تغییر سرویس سفارش
public interface IPaymentProcessor
{
    Task<PaymentResult> ProcessPaymentAsync(decimal amount , PaymentDetails details);
}


public record PaymentDetails(string CardNumber , string? Expiry , string? Cvv);
public record PaymentResult(bool Success , string TransactionId , string? ErrorMessage = null);