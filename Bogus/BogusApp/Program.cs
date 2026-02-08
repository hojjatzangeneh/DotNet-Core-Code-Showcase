using System.Text.Json;
using System.Text.Json.Serialization;

using Bogus;

using BogusApp.faker;
using BogusApp.models;

using Microsoft.EntityFrameworkCore;

/* =========================================================
   RANDOM SEED (تولید دیتای ثابت)
   ========================================================= */
Randomizer.Seed = new Random(123);

/* =========================================================
   ADDRESS FAKE
   ========================================================= */
var addressFaker = new Faker<Address>("fa")
    .RuleFor(o => o.Id , f => f.IndexFaker + 1)
    .RuleFor(a => a.City , f => f.Address.City())
    .RuleFor(a => a.Street , f => f.Address.StreetAddress())
    .RuleFor(a => a.Country , f => f.Address.Country())
    .RuleFor(a => a.ZipCode , f => f.Address.ZipCode());

/* =========================================================
   PRODUCT FAKE (CustomInstantiator)
   ========================================================= */
var productFaker = new Faker<Product>()
    .CustomInstantiator(f => new Product(Guid.NewGuid()))
    .RuleFor(p => p.Name , f => f.Commerce.ProductName())
    .RuleFor(p => p.Price , f => decimal.Parse(f.Commerce.Price(10 , 1000)));


/* =========================================================
   ORDER ITEM FAKE
   ========================================================= */
var orderItemFaker = new Faker<OrderItem>()
    .RuleFor(oi => oi.Product , f => productFaker.Generate())
    .RuleFor(oi => oi.Quantity , f => f.Random.Int(1 , 5))
    .RuleFor(oi => oi.UnitPrice , (f , oi) => oi.Product.Price);
/* =========================================================
   ORDER FAKE
   ========================================================= */
var orderFaker = new Faker<Order>()
    .RuleFor(o => o.Id , f => f.IndexFaker + 1)
    .RuleFor(o => o.OrderDate , f => f.Date.Past(2))
    .RuleFor(o => o.Items , f => orderItemFaker.Generate(f.Random.Int(1 , 5)));
/* =========================================================
   1️⃣ FLUENT API (روش اصلی Bogus)
   ========================================================= */

Console.WriteLine("\n--- Fluent API Example ---");

var userFaker = new Faker<User>("fa")
    .StrictMode(true)
    .RuleFor(u => u.Id , f => f.IndexFaker + 1)
    .RuleFor(u => u.FirstName , f => f.Name.FirstName())
    .RuleFor(u => u.LastName , f => f.Name.LastName())
    .RuleFor(u => u.FullName , (f , u) => $"{u.FirstName} {u.LastName}")
    .RuleFor(u => u.Email , f => f.Internet.Email())
    .RuleFor(u => u.Username , f => f.Internet.UserName())
    .RuleFor(u => u.BirthDate , f => f.Date.Past(30))
    .RuleFor(u => u.Address , f => addressFaker.Generate())
    .RuleFor(u => u.Orders , f => orderFaker.Generate(f.Random.Int(1 , 3)))
    .FinishWith((f , u) =>
    {
        Console.WriteLine($"User Created: {u.FullName} with {u.Orders.Count} orders");
    });

var users = userFaker.Generate(3);

foreach ( var u in users )
    Console.WriteLine($"{u.Id} - {u.FullName} - {u.Address.City}");

/* =========================================================
   2️⃣ FAKER FACADE (استفاده مستقیم از Dataset)
   ========================================================= */

Console.WriteLine("\n--- Faker Facade Example ---");

var faker = new Faker("fa");

Console.WriteLine(faker.Name.FullName());
Console.WriteLine(faker.Internet.Email());
Console.WriteLine(faker.Address.City());

/* =========================================================
   3️⃣ CUSTOM INSTANTIATOR
   ========================================================= */
Console.WriteLine("\n--- CustomInstantiator Example ---");
var products = productFaker.Generate(3);
foreach ( var p in products )
    Console.WriteLine($"{p.Id} - {p.Name}");

/* =========================================================
   4️⃣ STRICT MODE
   ========================================================= */

Console.WriteLine("\n--- StrictMode Example ---");

var strictUserFaker = new Faker<User>()
    .StrictMode(true)
    .RuleFor(u => u.Id , f => f.IndexFaker)
    .RuleFor(u => u.FirstName , f => f.Name.FirstName())
    .RuleFor(u => u.LastName , f => f.Name.LastName())
    .RuleFor(u => u.FullName , (f , u) => $"{u.FirstName} {u.LastName}")
    .RuleFor(u => u.Email , f => f.Internet.Email())
    .RuleFor(u => u.Username , f => f.Internet.UserName())
    .RuleFor(u => u.BirthDate , f => f.Date.Past(30))
    .RuleFor(u => u.Address , f => addressFaker.Generate())
    .RuleFor(u => u.Orders ,f => orderFaker.Generate(f.Random.Int(1 , 3)));

var strictUsers = strictUserFaker.Generate(2);

foreach ( var u in strictUsers )
    Console.WriteLine(u.FullName);


/* =========================================================
   5️⃣ FINISH WITH
   ========================================================= */

Console.WriteLine("\n--- FinishWith Example ---");

var finishFaker = new Faker<User>()
    .RuleFor(u => u.FirstName , f => f.Name.FirstName())
    .FinishWith((f , u) =>
    {
        Console.WriteLine($"User Created: {u.FirstName}");
    });

finishFaker.Generate(2);



/* =========================================================
   6️⃣ INHERITANCE (Enterprise Style)
   ========================================================= */

Console.WriteLine("\n--- Inheritance Faker Example ---");
var inheritedUsers = new UserFaker().Generate(3);
foreach ( var u in inheritedUsers )
    Console.WriteLine($"{u.Id} - {u.Email}");



/* =========================================================
   DISPLAY USERS WITH ORDERS AND ITEMS
   ========================================================= */
foreach ( var u in users )
{
    Console.WriteLine($"\nUser: {u.FullName} ({u.Email}) - {u.Address.City}");
    foreach ( var o in u.Orders )
    {
        Console.WriteLine($"  Order #{o.Id} - {o.OrderDate.ToShortDateString()} - Total: {o.TotalAmount:C}");
        foreach ( var item in o.Items )
        {
            Console.WriteLine($"    Product: {item.Product.Name} x{item.Quantity} @ {item.UnitPrice:C} each");
        }
    }
}

/* =========================================================
   FAKER FACADE EXAMPLE
   ========================================================= */
Console.WriteLine("\n--- Faker Facade Example ---");
var f = new Faker("fa");
Console.WriteLine(f.Name.FullName());
Console.WriteLine(f.Internet.Email());
Console.WriteLine(f.Address.City());

Console.WriteLine("\n=== END OF FULL BOGUS EXAMPLE ===");

/* =========================================================
   SEED DATABASE (BATCH INSERT)
   ========================================================= */

using AppDbContext db = new AppDbContext();

int batchSize = 50;
int totalUsers = 200;

for ( int i = 0 ; i < totalUsers / batchSize ; i++ )
{
    var usersDb = userFaker.Generate(batchSize);

    db.Users.AddRange(usersDb);
    db.SaveChanges();

    Console.WriteLine($"Inserted batch {i + 1}");
}

/* =========================================================
   READ DATA
   ========================================================= */

var usersFromDb = db.Users
    .Include(u => u.Orders)
    .ThenInclude(o => o.Items)
    .Take(3)
    .ToList();

foreach ( var u in usersFromDb )
{
    Console.WriteLine($"\nUser: {u.FirstName} {u.LastName}");

    foreach ( var o in u.Orders )
    {
        Console.WriteLine($"  Order: {o.OrderDate:d}");

        foreach ( var item in o.Items )
            Console.WriteLine($"    {item.Product.Name} x{item.Quantity}");
    }
}

Console.WriteLine("\n=== DONE ===");
Console.ReadLine();
