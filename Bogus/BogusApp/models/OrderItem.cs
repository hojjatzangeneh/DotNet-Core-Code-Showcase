using System;
using System.Collections.Generic;
using System.Text;

namespace BogusApp.models;

public class OrderItem
{
    public int Id { get; set; }
    public Product Product { get; set; } = new Product(Guid.NewGuid());
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}