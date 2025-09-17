using Serilog;
using Serilog.Sinks.Grafana.Loki;
using Serilog.Sinks.GrafanaLoki;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
string lokiUrl = "http://localhost:3100/loki/api/v1/push"; // اگر استک روی همین ماشین است

Log.Logger = new LoggerConfiguration()
    .MinimumLevel
    .Information()
    .Enrich
    .FromLogContext()
    .Enrich
    .WithEnvironmentName()
    .Enrich
    .WithProcessId()
    .Enrich
    .WithThreadId()
    .WriteTo
    .GrafanaLoki(
        lokiUrl,
        labels: new[]
        {
            new LokiLabel { Key = "app", Value = "dotnet-api" },
            new LokiLabel { Key = "env", Value = "dev" },
        })
    .WriteTo
    .Console() // برای مشاهده محلی
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();