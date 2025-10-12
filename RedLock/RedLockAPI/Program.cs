using RedLockAPI.DistributedLocking;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// add configuration / logging as usual

// create small temp logger for startup actions (so we can pass it to AddRedLockDistributedLockingAsync)
using var tempLoggerFactory = LoggerFactory.Create(lb => lb.AddConsole());
var startupLogger = tempLoggerFactory.CreateLogger("RedLockStartup");

// register configuration-backed options, then call async registration (await)
await builder.Services.AddRedLockDistributedLockingAsync(builder.Configuration , startupLogger);

var app = builder.Build();

// Configure the HTTP request pipeline.
if ( app.Environment.IsDevelopment() )
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/process/{id}" , async (string id , IDistributedLockService lockService , ILogger<Program> logger) =>
{
    await using var handle = await lockService.TryAcquireLockAsync($"locks:order:{id}");
    if ( !handle.IsAcquired )
        return Results.Conflict(new { message = "Resource is locked" });

    logger.LogInformation("Processing {Id}" , id);
    await Task.Delay(10000);
    return Results.Ok(new { message = "Processed" });
});

await app.RunAsync();
