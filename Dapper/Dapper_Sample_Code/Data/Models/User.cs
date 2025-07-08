
namespace Dapper_Sample_Code.Data.Models;

public class User
{
    public string Email { get; set; }

    public int Id { get; set; }

    public string Name { get; set; }

    public Profile Profile { get; set; }
}