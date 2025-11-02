using EfHiLoSqlServer;

using var context = new AppDbContext();
context.Database.EnsureDeleted();
context.Database.EnsureCreated();

for ( int i = 1 ; i <= 20 ; i++ )
{
    context.Products.Add(new Product { Name = $"Product {i}" });
}

context.SaveChanges();

foreach ( var product in context.Products )
{
    Console.WriteLine($"Id: {product.Id}, Name: {product.Name}");
}
Console.WriteLine("Done.");
Console.ReadLine();
