using System;
using System.Collections.Generic;
using System.Text;

using Bogus.DataSets;

namespace BogusApp.models;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public Address Address { get; set; } = new();
    public List<Order> Orders { get; set; } = [];
}