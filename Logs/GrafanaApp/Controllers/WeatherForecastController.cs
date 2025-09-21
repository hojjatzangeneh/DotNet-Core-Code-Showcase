
using Microsoft.AspNetCore.Mvc;

namespace GrafanaApp.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController(ILogger<WeatherForecastController> logger) : ControllerBase
{
    static readonly string[] Summaries =
    [
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
    ];

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        logger.LogInformation("WeatherForecast/Get called at {Time}", DateTime.UtcNow);

        WeatherForecast[] result = Enumerable.Range(1, 5)
            .Select(
                index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
            .ToArray();

        logger.LogInformation("Generated {Count} weather records", result.Length);
        logger.LogDebug("پیام دیباگ");
        logger.LogInformation("درخواست جدید برای WeatherForecast");
        logger.LogWarning("هشدار! دما خیلی بالا بود: {Temp}", "temp");
        logger.LogError("خطا هنگام دریافت اطلاعات: {Error}", "ex.Message");

        return result;
    }
}