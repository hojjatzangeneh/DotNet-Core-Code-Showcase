using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Service info
var serviceName = "OpenTelemetrySimpleService";
var serviceVersion = "1.0.0";

// Add logging (built-in)
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add controllers
builder.Services.AddControllers();

// Create an ActivitySource and a Meter for manual instrumentation
var activitySource = new ActivitySource("SimpleApp.ActivitySource");
var meter = new System.Diagnostics.Metrics.Meter("SimpleApp.Meter" , "1.0.0");
var requestCounter = meter.CreateCounter<long>("simple_requests_total" , description: "Total requests to sample endpoint");



var responseCounter = "";
// Register meter so controller can receive it via DI
builder.Services.AddSingleton(activitySource);
builder.Services.AddSingleton(meter);

// Configure OpenTelemetry: traces + metrics -> Console exporter
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: serviceName , serviceVersion: serviceVersion))
        .AddAspNetCoreInstrumentation(options =>
        {
            //options.Enrich = (activity , eventName , obj) =>
            //{
            //    // optional: attach route or other info
            //};
            options.RecordException = true;
        })
        .AddHttpClientInstrumentation()
        .AddSource("SimpleApp.ActivitySource")
        .AddConsoleExporter() // exports traces to console
    )
    .WithMetrics(metrics => metrics
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation() // GC, threads, etc.
        .AddMeter("SimpleApp.Meter")
        .AddConsoleExporter() // exports metrics to console (aggregation snapshots)
    );

var app = builder.Build();

app.MapControllers();

app.MapGet("/" , (ILogger<Program> logger) =>
{
    // simple log showing no HTTP trace
    logger.LogInformation("Root endpoint hit");
    return Results.Ok("Hello from OpenTelemetry simple app");
});

app.Run();
