using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FoodOrdering.Application.Payments;

namespace FoodOrdering.Application.Orders;
public interface IOrderService
{
    Task<Guid> PlaceOrderAsync(CreateOrderDto dto);
}


public record CreateOrderDto(string CustomerEmail , List<CreateOrderItemDto> Items , PaymentDetails Payment);
public record CreateOrderItemDto(Guid MenuItemId , int Quantity);