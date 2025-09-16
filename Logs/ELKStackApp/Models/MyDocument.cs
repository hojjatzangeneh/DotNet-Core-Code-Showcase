
namespace ELKStackApp.Models;

public class MyDocument
{
    public string Description { get; set; }

    public int Id { get; set; } = new Random().Next();

    public string Name { get; set; }

    public string Title { get; set; }
}