using Memory_Cache.Models;
using Memory_Cache.Services;

using Microsoft.AspNetCore.Mvc;

namespace Memory_Cache.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class BaseController <TService>(TService service) : ControllerBase where TService : IProductService
{
    [HttpGet("products/{id:int}")]
    public async Task<ActionResult<Product>> GetProduct([FromRoute] int id, CancellationToken cancellationToken)
    {
        Product? product = await service.GetAsync(id, cancellationToken);
        return (product is null) ? NotFound() : Ok(product);
    }

    [HttpPost("products/{id:int}/price")]
    public async Task<IActionResult> UpdatePrice(
        [FromRoute] int id,
        [FromBody] decimal price,
        CancellationToken cancellationToken)
    {
        await service.UpdatePriceAsync(id, price, cancellationToken);
        return NoContent();
    }
}