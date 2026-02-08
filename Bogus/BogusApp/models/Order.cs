using System;
using System.Collections.Generic;
using System.Text;

namespace BogusApp.models;

public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public decimal TotalAmount => CalculateTotal();

    private decimal CalculateTotal()
    {
        decimal sum = 0;
        foreach ( var item in Items )
            sum += item.Quantity * item.UnitPrice;
        return sum;
    }
}
