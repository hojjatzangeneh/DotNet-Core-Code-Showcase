using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
{
    // Add services to the container.
    _ = "http://localhost:8130/loki/api/v1/push"; // اگر استک روی همین ماشین است
}

// Log.Logger = new LoggerConfiguration()
// .MinimumLevel
// .Information()
// .Enrich
// .FromLogContext()
// .Enrich
// .WithEnvironmentName()
// .Enrich
// .WithProcessId()
// .Enrich
// .WithThreadId()
// .WriteTo
// .GrafanaLoki(
// lokiUrl,
// labels: new[]
// {
// new LokiLabel { Key = "app", Value = "dotnet-api" },
// new LokiLabel { Key = "env", Value = "dev" },
// })
// .WriteTo
// .Console() // برای مشاهده محلی
// .CreateLogger();

// builder.Host.UseSerilog();
builder.Host
    .UseSerilog(
        (context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

WebApplication app = builder.Build();
app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();