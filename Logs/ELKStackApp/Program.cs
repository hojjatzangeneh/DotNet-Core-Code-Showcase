using Microsoft.Extensions.Primitives;

using Serilog;
using Serilog.Context;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Host.AddSerilog();
IServiceCollection services = builder.Services;
services.AddEndpointsApiExplorer();
services.AddControllers();

WebApplication app = builder.Build();
app.Use(
    async (context, next) =>
    {
        StringValues userId = context.Request.Headers["UserId"]!;
        using(LogContext.PushProperty("UserId", userId))
        {
            await next.Invoke();
        }
    });
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.MapGet(
    "/",
    (ILogger<Program> logger) =>
    {
        logger.LogInformation("Section 01");
        string[] summaries = new[]
        {
            "Freezing",
            "Bracing",
            "Chilly",
            "Cool",
            "Mild",
            "Warm",
            "Balmy",
            "Hot",
            "Sweltering",
            "Scorching"
        };
        logger.LogInformation("Section 02");
        WeatherForecastDto[] forecast = Enumerable.Range(1, 5)
            .Select(
                index =>
                new WeatherForecastDto(
                        DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        Random.Shared.Next(-20, 55),
                        summaries[Random.Shared.Next(summaries.Length)]))
            .ToArray();
        logger.LogInformation("Section 03");
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();
app.MapControllers();
app.Run();