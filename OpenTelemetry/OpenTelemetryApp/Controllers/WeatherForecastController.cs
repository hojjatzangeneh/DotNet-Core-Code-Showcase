using System.Diagnostics;

using Microsoft.AspNetCore.Mvc;

namespace OpenTelemetryApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly ActivitySource _activitySource;
    private readonly System.Diagnostics.Metrics.Meter _meter;
    private readonly System.Diagnostics.Metrics.Counter<long> _counter;
    private readonly ILogger<WeatherController> _logger;

    public WeatherController(ActivitySource activitySource , System.Diagnostics.Metrics.Meter meter , ILogger<WeatherController> logger)
    {
        _activitySource = activitySource;
        _meter = meter;
        _counter = _meter.CreateCounter<long>("weather_requests_total" , description: "Total weather requests");
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        // Start a new Activity (span)
        using var activity = _activitySource.StartActivity("Weather.Get" , ActivityKind.Server);

        // add tags to the trace
        activity?.SetTag("weather.endpoint" , "Get");
        activity?.SetTag("app.version" , "1.0.0");

        // increment the custom metric
        _counter.Add(1);

        // Log something and include current TraceId if available
        var traceId = activity?.TraceId.ToString() ?? "no-trace";
        _logger.LogInformation("Weather.Get called. traceId={TraceId}" , traceId);

        // simulate small work (non-blocking recommended in real apps)
        System.Threading.Thread.Sleep(50);

        var result = new { TemperatureC = 22 , Summary = "Partly Cloudy" , TraceId = traceId };
        return Ok(result);
    }
}