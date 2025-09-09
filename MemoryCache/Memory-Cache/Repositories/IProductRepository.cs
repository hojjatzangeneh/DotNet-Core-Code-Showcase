using Memory_Cache.Models;

namespace Memory_Cache.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);

    Task UpdatePriceAsync(int id, decimal price, CancellationToken ct = default);
}