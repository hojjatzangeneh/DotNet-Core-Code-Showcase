using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FoodOrdering.Application.Repositories;
using FoodOrdering.Domain.Entities;

namespace FoodOrdering.Infrastructure.Repositories;
// یک پیاده سازی ساده برای تست و دمو؛ می توان به EFCore تغییر داد بدون تغییر لایه های بالاتر (DIP)
public class InMemoryOrderRepository : IOrderRepository
{
    private readonly List<Order> _orders = new();
    public Task AddAsync(Order entity)
    {
        _orders.Add(entity);
        return Task.CompletedTask;
    }
    public Task<Order?> GetByIdAsync(Guid id) => Task.FromResult(_orders.FirstOrDefault(o => o.Id == id));
    public Task<IEnumerable<Order>> ListAsync() => Task.FromResult<IEnumerable<Order>>(_orders);
    public Task<IEnumerable<Order>> GetByCustomerEmailAsync(string email) => Task.FromResult(_orders.Where(o => o.CustomerEmail == email).AsEnumerable());
    public Task SaveChangesAsync() => Task.CompletedTask;
}
