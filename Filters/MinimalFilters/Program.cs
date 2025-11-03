using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using MinimalFilters.Filters;
using MinimalFilters.Models;

var builder = WebApplication.CreateBuilder(args);

// Add controllers so we can demonstrate MVC filters (Authorization/Resource/Action/Result/Exception)
builder.Services.AddControllers(options =>
{
    // Register global MVC filters (applies to controllers)
    options.Filters.Add<CustomAuthorizationFilter>();   // Authorization filter
    options.Filters.Add<CustomResourceFilter>();        // Resource filter
    options.Filters.Add<CustomActionFilter>();          // Action filter
    options.Filters.Add<CustomResultFilter>();          // Result filter
    options.Filters.Add<GlobalExceptionFilter>();       // Exception filter
});

// If any services are needed in filters, register them here
builder.Services.AddSingleton<HInMemoryCache>();

var app = builder.Build();

// Map a minimal endpoint with an Endpoint Filter (for Minimal API)
app.MapGet("/minimal/products/{id:int}" , (int id) =>
{
    // This is the endpoint handler (action)
    var p = new Product { Id = id , Name = $"Product-{id}" , Price = 9.99m * id };
    return Results.Ok(p);
})
.AddEndpointFilter(async (endpointContext , next) =>
{
    // EndpointFilter runs before and/or after the endpoint delegate
    var httpContext = endpointContext.HttpContext;

    // Example: simple "authorization" inside endpoint filter
    if ( !httpContext.Request.Headers.TryGetValue("X-Api-Key" , out var key) || key != "secret" )
    {
        return Results.Unauthorized();
    }

    // Example: validation before endpoint
    var id = endpointContext.Arguments.FirstOrDefault();
    if ( id is int i && i <= 0 )
        return Results.BadRequest("id must be > 0");

    // call the endpoint
    var result = await next(endpointContext);

    // after-next: for example, wrap the result or modify
    return result;
});

// Map MVC controller endpoints
app.MapControllers();

app.Run();
