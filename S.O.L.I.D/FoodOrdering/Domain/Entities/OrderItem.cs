using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodOrdering.Domain.Entities;
public sealed class OrderItem
{
    public Guid MenuItemId { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal TotalPrice => UnitPrice * Quantity;
}