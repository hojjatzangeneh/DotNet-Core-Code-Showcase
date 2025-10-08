using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FoodOrdering.Application.Notifications;
using FoodOrdering.Application.Payments;
using FoodOrdering.Application.Repositories;
using FoodOrdering.Domain.Entities;

namespace FoodOrdering.Application.Orders;
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentProcessor _paymentProcessor;
    private readonly INotificationService _notificationService;


    // DIP: وابستگی ها از بیرون تزریق می شوند (به abstraction ها)

    public OrderService(IOrderRepository orderRepository , IPaymentProcessor paymentProcessor , INotificationService notificationService)
    {
        _orderRepository = orderRepository;
        _paymentProcessor = paymentProcessor;
        _notificationService = notificationService;
    }


    public async Task<Guid> PlaceOrderAsync(CreateOrderDto dto)
    {
        // SRP: این متد فقط جریان قرار دادن سفارش را مدیریت می کند؛ اعتبارسنجی / ارسال ایمیل / پرداخت هر کدام کلاس خود را دارند


        // Validate (می توان Validator جداگانه آورد)
        if ( dto.Items == null || !dto.Items.Any() )
            throw new ArgumentException("Order must contain items");


        // Map DTO -> Domain
        var order = new Order { CustomerEmail = dto.CustomerEmail };
        foreach ( var item in dto.Items )
        {
            // در مثال واقعی قیمت باید از منوب آیتم خوانده شود؛ اینجا برای خلاصه سازی ثابت می گیریم
            order.Items.Add(new OrderItem { MenuItemId = item.MenuItemId , Quantity = item.Quantity , UnitPrice = 10m });
        }

        // Payment
        var paymentResult = await _paymentProcessor.ProcessPaymentAsync(order.Total , new PaymentDetails(dto.Payment.CardNumber , dto.Payment.Expiry , dto.Payment.Cvv));
        if ( !paymentResult.Success )
            throw new InvalidOperationException("Payment failed: " + paymentResult.ErrorMessage);
        
        // Persist
        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();
        // Notify
        await _notificationService.SendOrderConfirmationAsync(order.CustomerEmail , order.Id);
        return order.Id;
    }
}
