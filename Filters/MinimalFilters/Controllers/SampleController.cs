using Microsoft.AspNetCore.Mvc;

using MinimalFilters.Models;

namespace MinimalFilters.Controllers;
[ApiController]
[Route("api/[controller]")]
public class SampleController : ControllerBase
{
    [HttpGet("product/{id:int}")]
    public IActionResult GetProduct(int id)
    {
        // این اکشن تحت تاثیر Authorization/Resource/Action/Result/Exception (همه) خواهد بود
        if ( id == 13 )
        {
            // مثال خطا که ExceptionFilter آن را هندل می‌کند
            throw new InvalidOperationException("No product with id 13 allowed!");
        }

        var p = new Product { Id = id , Name = $"Product-{id}" , Price = 3.14m * id };
        return Ok(p); // ResultFilter اجرا خواهد شد
    }
}
