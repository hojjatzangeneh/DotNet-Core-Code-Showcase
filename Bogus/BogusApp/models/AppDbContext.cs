using System;
using System.Collections.Generic;
using System.Text;

using Bogus;

using Microsoft.EntityFrameworkCore;
namespace BogusApp.models;

public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseInMemoryDatabase("BogusDb");
}
