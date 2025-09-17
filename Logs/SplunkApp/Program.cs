using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Handler فقط در DEV برای بی‌اثر کردن چکِ گواهی
HttpMessageHandler? insecureHandler = null;
if(builder.Environment.IsDevelopment())
{
    insecureHandler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
}

Log.Logger = new LoggerConfiguration()
    .ReadFrom
    .Configuration(builder.Configuration) // Console و بقیه از appsettings
    .WriteTo
    .EventCollector(                       // فقط HEC را برنامه‌نویسی اضافه می‌کنیم تا handler بدهیم
        splunkHost: "https://localhost:8088",
        // port: 8088 ,
        // useSSL: true ,
        uriPath: "services/collector",
        eventCollectorToken: "92f8075a-f877-498b-9a50-6aba86fd86c4",
        index: "app_logs",
        sourceType: "_json",
        messageHandler: insecureHandler
    )
    .CreateLogger();

builder.Host.UseSerilog();
// Add services to the container.

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

await app.RunAsync();