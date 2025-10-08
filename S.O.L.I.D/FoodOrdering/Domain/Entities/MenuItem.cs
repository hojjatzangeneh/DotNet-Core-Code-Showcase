using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodOrdering.Domain.Entities;
public sealed class MenuItem
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public decimal Price { get; init; }
}