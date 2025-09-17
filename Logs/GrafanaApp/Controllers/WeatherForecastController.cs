
using Microsoft.AspNetCore.Mvc;

namespace GrafanaApp.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    static readonly string[] Summaries = new[]
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

    readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        _logger.LogInformation("WeatherForecast/Get called at {Time}", DateTime.UtcNow);

        WeatherForecast[] result = Enumerable.Range(1, 5)
            .Select(
                index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
            .ToArray();

        _logger.LogInformation("Generated {Count} weather records", result.Length);
        _logger.LogDebug("پیام دیباگ");
        _logger.LogInformation("درخواست جدید برای WeatherForecast");
        _logger.LogWarning("هشدار! دما خیلی بالا بود: {Temp}", "temp");
        _logger.LogError("خطا هنگام دریافت اطلاعات: {Error}", "ex.Message");

        return result;
    }
}