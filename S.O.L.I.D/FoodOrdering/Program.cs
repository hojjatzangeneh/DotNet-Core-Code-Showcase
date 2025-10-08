// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FoodOrdering.Application.Orders;
using FoodOrdering.Application.Payments;
using FoodOrdering.Application.Notifications;
using FoodOrdering.Application.Repositories;
using FoodOrdering.Infrastructure.Payments;
using FoodOrdering.Infrastructure.Notifications;
using FoodOrdering.Infrastructure.Repositories;


var host = Host.CreateDefaultBuilder(args)
.ConfigureServices((ctx , services) => {
    // Registrations (DIP)
    services.AddSingleton<IOrderRepository , InMemoryOrderRepository>();
    services.AddTransient<IPaymentProcessor , StripePaymentProcessor>();
    services.AddTransient<INotificationService , EmailNotificationService>();
    services.AddTransient<IOrderService , OrderService>();
})
.Build();


// Demo usage (Console)
using var scope = host.Services.CreateScope();
var svc = scope.ServiceProvider.GetRequiredService<IOrderService>();


var dto = new CreateOrderDto(
CustomerEmail: "customer@example.com" ,
Items: [new(Guid.NewGuid() , 2)] ,
Payment: new PaymentDetails("4242-4242-4242-4242" , "12/26" , "123")
);


var orderId = await svc.PlaceOrderAsync(dto);
Console.WriteLine($"Order placed: {orderId}");

// ---------- Notes: چگونه این مثال SOLID را رعایت می کند ----------
// SRP: هر کلاس تنها یک مسئولیت دارد (Repository فقط ذخیره، Notification فقط ایمیل، OrderService مدیریت جریان)
// OCP: افزودن روش پرداخت جدید با پیاده سازی IPaymentProcessor امکان پذیر است بدون تغییر OrderService
// LSP: پیاده سازی های IPaymentProcessor باید قرارداد را نقض نکنند (مثلاً هر پردازشگر باید ProcessPaymentAsync را مطابق انتظار اجرا کند)
// ISP: اینترفیس ها کوچک و تخصصی هستند (IRepository، IReadableRepository)
// DIP: لایه ی Application به پیاده سازی ها وابسته نیست؛ وابستگی ها از طریق abstractionها تزریق می شوند