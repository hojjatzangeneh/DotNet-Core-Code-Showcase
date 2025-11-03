using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqWorkshop;
#region Sample Domain Models
public record Person(int Id , string Name , int Age , string City);
public record Order(int Id , int PersonId , int ProductId , int Quantity , decimal Price , DateTime Date);
public record Product(int Id , string Name , string Category , decimal UnitPrice);
#endregion