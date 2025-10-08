using CorrelationIdSample.Background;
using CorrelationIdSample.Handlers;
using CorrelationIdSample.Middleware;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Compact;

var logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} (cid:{CorrelationId}){NewLine}{Exception}")
    .CreateLogger();

Log.Logger = logger;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// DI
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CorrelationIdPropagationHandler>();
builder.Services.AddSingleton<SimpleBackgroundQueue>();
builder.Services.AddHostedService<BackgroundQueueRunner>(); // background runner for demo

builder.Services.AddHttpClient("demo")
    .AddHttpMessageHandler<CorrelationIdPropagationHandler>();

builder.Services.AddControllers();

var app = builder.Build();

// IMPORTANT: Use correlation middleware as early as possible
app.UseCorrelationId();

app.MapControllers();

app.Run();