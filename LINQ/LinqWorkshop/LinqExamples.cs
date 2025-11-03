// LinqExamples Library
// A single-file class library demonstrating advanced, professional LINQ patterns
// Author: Generated for user

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LinqWorkshop;

/// <summary>
/// Comprehensive LINQ examples collected as static methods. Each method returns
/// a sample result or demonstrates behavior. Use these from unit tests or a demo app.
/// </summary>
public static class LinqExamples
{
    #region Models
    public record Person(int Id , string Name , int Age , string City);
    public record Product(int Id , string Name , string Category , decimal UnitPrice);
    public record Order(int Id , int PersonId , int ProductId , int Quantity , decimal Price , DateTime Date);
    #endregion

    #region SeedData
    public static List<Person> People { get; } = new()
    {
        new Person(1, "Alice", 30, "Seattle"),
        new Person(2, "Bob", 42, "Portland"),
        new Person(3, "Charlie", 25, "Seattle"),
        new Person(4, "Diana", 35, "Vancouver"),
        new Person(5, "Eve", 29, "Portland")
    };

    public static List<Product> Products { get; } = new()
    {
        new Product(1, "Keyboard", "Peripherals", 29.99m),
        new Product(2, "Mouse", "Peripherals", 19.99m),
        new Product(3, "Monitor", "Displays", 159.99m),
        new Product(4, "Laptop", "Computers", 1299.99m),
        new Product(5, "USB-C Cable", "Accessories", 9.99m)
    };

    public static List<Order> Orders { get; } = new()
    {
        new Order(1, 1, 4, 1, 1299.99m, DateTime.UtcNow.AddDays(-10)),
        new Order(2, 1, 3, 2, 159.99m, DateTime.UtcNow.AddDays(-5)),
        new Order(3, 2, 2, 5, 19.99m, DateTime.UtcNow.AddDays(-8)),
        new Order(4, 3, 5, 3, 9.99m, DateTime.UtcNow.AddDays(-2)),
        new Order(5, 4, 1, 2, 29.99m, DateTime.UtcNow.AddDays(-1))
    };
    #endregion

    #region BasicFilteringAndProjection
    public static IEnumerable<string> WhereAndSelectExample()
    {
        return People
            .Where(p => p.City == "Seattle" && p.Age > 26)
            .OrderByDescending(p => p.Age)
            .Select(p => p.Name.ToUpper())
            .ToList();
    }

    public static IEnumerable<string> QuerySyntaxExample()
    {
        var q = from p in People
                where p.City.StartsWith("P")
                orderby p.Name
                select $"{p.Name} ({p.Age})";
        return q.ToList();
    }
    #endregion

    #region GroupingAndAggregation
    public static IEnumerable<(string City, int Count, double AverageAge)> GroupByExample()
    {
        return People
            .GroupBy(p => p.City)
            .Select(g => (City: g.Key, Count: g.Count(), AverageAge: g.Average(p => p.Age)))
            .OrderByDescending(r => r.Count)
            .ToList();
    }

    public static (decimal TotalRevenue, decimal MaxOrder, decimal MinOrder) AggregateExample()
    {
        var totals = Orders.Select(o => o.Price * o.Quantity);

        var totalRevenue = totals.Sum();
        var max = totals.DefaultIfEmpty(0m).Max();
        var min = totals.DefaultIfEmpty(0m).Min();

        var productQty = Orders.Select(o => o.Quantity).Aggregate(1 , (acc , v) => acc * v);

        return (TotalRevenue: Math.Round(totalRevenue , 2), MaxOrder: max, MinOrder: min);
    }
    #endregion

    #region JoinsAndSelectMany
    public static IEnumerable<object> InnerJoinExample()
    {
        var q = from o in Orders
                join p in Products on o.ProductId equals p.Id
                select new
                {
                    o.Id ,
                    Person = People.FirstOrDefault(x => x.Id == o.PersonId)?.Name ?? "Unknown" ,
                    Product = p.Name ,
                    Total = o.Quantity * o.Price ,
                    o.Date
                };
        return q.ToList();
    }

    public static IEnumerable<object> LeftJoin_GroupJoinExample()
    {
        var q = from person in People
                join order in Orders on person.Id equals order.PersonId into gj
                select new
                {
                    person.Name ,
                    Orders = gj.OrderByDescending(o => o.Date).Select(o => new { o.Id , o.Quantity , o.ProductId })
                };
        return q.ToList();
    }

    public static IEnumerable<object> SelectManyExample()
    {
        var q = People.SelectMany(
            p => Products.Where(pr => pr.Category == "Peripherals") ,
            (person , product) => new { PersonName = person.Name , ProductName = product.Name , ProductCategory = product.Category });

        return q.ToList();
    }
    #endregion

    #region SetOperationsAndComparers
    public static IEnumerable<Product> DistinctCustomExample()
    {
        return Products.Distinct(new ProductCategoryComparer()).ToList();
    }

    public static IEnumerable<Product> UnionExceptIntersectExample()
    {
        var a = Products.Where(p => p.UnitPrice < 100);
        var b = Products.Where(p => p.Category == "Peripherals");

        var union = a.Union(b , new ProductComparer());
        var except = a.Except(b , new ProductComparer());
        var intersect = a.Intersect(b , new ProductComparer());

        return union.ToList();
    }

    private class ProductCategoryComparer : IEqualityComparer<Product>
    {
        public bool Equals(Product? x , Product? y) => x != null && y != null && x.Category == y.Category;
        public int GetHashCode(Product obj) => obj.Category?.GetHashCode() ?? 0;
    }

    private class ProductComparer : IEqualityComparer<Product>
    {
        public bool Equals(Product? x , Product? y) => x != null && y != null && x.Id == y.Id;
        public int GetHashCode(Product obj) => obj.Id.GetHashCode();
    }
    #endregion

    #region PartitioningOrderingPaging
    public static IEnumerable<Person> SkipTakeExample(int page , int pageSize)
    {
        return People.OrderBy(p => p.Id).Skip(( page - 1 ) * pageSize).Take(pageSize).ToList();
    }

    public static IEnumerable<Person> ThenByExample()
    {
        return People.OrderBy(p => p.City).ThenByDescending(p => p.Age).ToList();
    }
    #endregion

    #region Lookup
    public static ILookup<string , Person> ToLookupExample()
    {
        return People.ToLookup(p => p.City);
    }
    #endregion

    #region ExecutionAndDeferred
    public static (IEnumerable<Person> QueryBeforeMutation, IEnumerable<Person> MaterializedAfter) DeferredExecutionDemo()
    {
        var query = People.Where(p => p.Age > 28);

        People.Add(new Person(6 , "Frank" , 50 , "Seattle"));

        var resultDeferred = query.ToList();

        var query2 = People.Where(p => p.Age > 28).ToList();
        People.Add(new Person(7 , "Gina" , 60 , "Portland"));

        return (QueryBeforeMutation: resultDeferred, MaterializedAfter: query2);
    }
    #endregion

    #region ConversionAndExecutionOperators
    public static (int Count, bool AnySeattle, Person? First) ExecutionOperatorsExample()
    {
        var count = People.Count();
        var anySeattle = People.Any(p => p.City == "Seattle");
        var first = People.FirstOrDefault(p => p.Age > 100);
        return (count, anySeattle, first);
    }
    #endregion

    #region ExpressionTreesAndIQueryable
    public static Expression<Func<Person , bool>> BuildPredicate(string city , int minAge)
    {
        var param = Expression.Parameter(typeof(Person) , "p");
        var propCity = Expression.Property(param , nameof(Person.City));
        var propAge = Expression.Property(param , nameof(Person.Age));

        var cityConst = Expression.Constant(city);
        var minAgeConst = Expression.Constant(minAge);

        var cityEq = Expression.Equal(propCity , cityConst);
        var ageGte = Expression.GreaterThanOrEqual(propAge , minAgeConst);
        var and = Expression.AndAlso(cityEq , ageGte);

        return Expression.Lambda<Func<Person , bool>>(and , param);
    }

    public static IEnumerable<Person> ApplyExpressionExample(string city , int minAge)
    {
        var predicate = BuildPredicate(city , minAge).Compile();
        return People.Where(predicate).ToList();
    }
    #endregion

    #region AsyncAndEfCoreRemarks
    public static async Task<List<Order>> EfCoreAsyncExample(IQueryable<Order> ordersQuery)
    {
        await Task.Delay(1);
        return ordersQuery.Where(o => o.Date > DateTime.UtcNow.AddMonths(-1)).ToList();
    }
    #endregion

    #region Utilities
    public static IEnumerable<(Person Person, decimal TotalSpent)> AggregatePerPerson()
    {
        var q = from o in Orders
                group o by o.PersonId into g
                join p in People on g.Key equals p.Id
                select (Person: p, TotalSpent: g.Sum(o => o.Price * o.Quantity));
        return q.OrderByDescending(x => x.TotalSpent).ToList();
    }

    public static IEnumerable<object> ComplexQueryExample()
    {
        var q = Orders
            .Where(o => o.Date > DateTime.UtcNow.AddDays(-30))
            .GroupBy(o => o.PersonId)
            .Select(g => new
            {
                Person = People.FirstOrDefault(p => p.Id == g.Key)?.Name ,
                OrderCount = g.Count() ,
                Total = g.Sum(o => o.Price * o.Quantity) ,
                Products = g.Select(o => Products.FirstOrDefault(p => p.Id == o.ProductId)?.Name).Distinct()
            })
            .OrderByDescending(x => x.Total);

        return q.ToList();
    }

    public static IEnumerable<Person> FilterAdults()
    {
        return People.Where(p => p.Age >= 18).ToList();
    }

    public static IEnumerable<IGrouping<int , Order>> GroupOrdersByPerson()
    {
        return Orders.GroupBy(o => o.PersonId).ToList();
    }

    public static IEnumerable<(string PeopleName, decimal Amount)> JoinPeopleWithOrders()
    {
        var query = from p in People
                    join o in Orders on p.Id equals o.PersonId
                    select (p.Name, o.Price);

        return query.ToList();
    }

    public static decimal CalculateTotalSales()
    {
        return Orders.Aggregate(0m , (total , o) => total + o.Price);
    }
    #endregion
}
