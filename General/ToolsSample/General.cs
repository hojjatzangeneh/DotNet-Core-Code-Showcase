using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IO;

using ToolsSample.Database;
using Microsoft.IO;

namespace ToolsSample;

public class General
{
    private async Task AsNoTrackingWithIdentityResolution()
    {
        using var context = new AppDbContext(new());

        var orders = await context.Orders
    .Include(o => o.Customer)
    .AsNoTracking()
    .ToListAsync();
        var orders2 = await context.Orders
    .Include(o => o.Customer)
    .AsNoTrackingWithIdentityResolution()
    .ToListAsync();
    }
    private void RecyclableMemoryStreamManager()
    {

        var manager = new RecyclableMemoryStreamManager();
        // گرفتن یک stream جدید از pool
        using ( var stream = manager.GetStream() )
        {
            var writer = new StreamWriter(stream);
            writer.Write("Hello World");
            writer.Flush();

            stream.Position = 0;
            var reader = new StreamReader(stream);
            Console.WriteLine(reader.ReadToEnd());
        }
    }
}

