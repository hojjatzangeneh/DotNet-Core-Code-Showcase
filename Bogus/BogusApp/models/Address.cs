using System;
using System.Collections.Generic;
using System.Text;

namespace BogusApp.models;

public class Address
{
    public int Id { get; set; }
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
}
