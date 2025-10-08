using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodOrdering.Domain.Entities;
public class Order
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public List<OrderItem> Items { get; } = new();
    public decimal Total => Items.Sum(i => i.TotalPrice);
    public string? CustomerEmail { get; init; }
}
