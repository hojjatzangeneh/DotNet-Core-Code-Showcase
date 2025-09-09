using Memory_Cache.Models;

namespace Memory_Cache.Repositories;

/// <summary>
/// Fake repository: simulates I/O latency and an in-memory store. In real life, this would be EF Core / Dapper /
/// external API.
/// </summary>
public class ProductRepository : IProductRepository
{
    static readonly Dictionary<int, Product> database = new()
    {
        [1] = new Product { Id = 1, Name = "Laptop Pro 14", Price = 1899_000, Category = "Electronics" },
        [2] = new Product { Id = 2, Name = "Noise Cancelling Headphones", Price = 599_000, Category = "Electronics" },
        [3] = new Product { Id = 3, Name = "Running Shoes", Price = 299_000, Category = "Sport" },
    };

    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        // Simulate latency
        await Task.Delay(250, ct);
        database.TryGetValue(id, out Product? p);
        return p;
    }

    public async Task UpdatePriceAsync(int id, decimal price, CancellationToken ct = default)
    {
        await Task.Delay(150, ct);
        if(database.TryGetValue(id, out Product? p))
        {
            database[id] = p with { Price = price , UpdatedAtUtc = DateTime.UtcNow };
        }
    }
}