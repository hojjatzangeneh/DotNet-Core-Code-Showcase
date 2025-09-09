
namespace Memory_Cache.Models;

public record Product
{
    public string Category { get; init; } = "General";

    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public decimal Price { get; init; }

    public DateTime UpdatedAtUtc { get; init; } = DateTime.UtcNow;
}