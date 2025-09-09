using Memory_Cache.Models;

namespace Memory_Cache.Services;

public interface IProductService
{
    Task<Product?> GetAsync(int id, CancellationToken ct = default);

    Task UpdatePriceAsync(int id, decimal price, CancellationToken ct = default);
}