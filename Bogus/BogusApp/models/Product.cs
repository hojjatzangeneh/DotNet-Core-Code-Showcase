using System;
using System.Collections.Generic;
using System.Text;

namespace BogusApp.models;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public Product() { } // EF Core
    public Product(Guid id)
    {
        Id = id;
    }
}
