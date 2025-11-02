using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsSample.Model;
public class Order
{
    public int Id { get; set; }
    public string Code { get; set; }
    public Customer Customer { get; set; }
}