using System;
using System.Collections.Generic;
using System.Text;

using Bogus;

using BogusApp.models;

namespace BogusApp.faker;

public class UserFaker : Faker<User>
{
    public UserFaker()
    {
        RuleFor(u => u.Id , f => f.IndexFaker + 1);
        RuleFor(u => u.FirstName , f => f.Name.FirstName());
        RuleFor(u => u.LastName , f => f.Name.LastName());
        RuleFor(u => u.FullName , (f , u) => $"{u.FirstName} {u.LastName}");
        RuleFor(u => u.Email , f => f.Internet.Email());
        RuleFor(u => u.Username , f => f.Internet.UserName());
        RuleFor(u => u.BirthDate , f => f.Date.Past(25));
        RuleFor(u => u.Address , f => new Address
        {
            City = f.Address.City() ,
            Street = f.Address.StreetAddress() ,
            Country = f.Address.Country() ,
            ZipCode = f.Address.ZipCode()
        });
        RuleFor(u => u.Orders , f => new List<Order>());
    }
}