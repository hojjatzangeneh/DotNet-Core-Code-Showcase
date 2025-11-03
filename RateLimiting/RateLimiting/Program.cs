using System.Threading.RateLimiting;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add Rate Limiter
builder.Services.AddRateLimiter(static options =>
{
    // 1) Global token-bucket (default) — برای همه endpointها، اجازه 20 درخواست و بازسازی 1 توکن در هر 50ms
    options.AddTokenBucketLimiter("global-token" , opt =>
    {
        opt.TokenLimit = 20;             // ظرفیت سطل
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10;             // چند درخواست در صف می ماند
        opt.ReplenishmentPeriod = TimeSpan.FromMilliseconds(50);
        opt.TokensPerPeriod = 1;
        opt.AutoReplenishment = true;
    });

    // 2) Fixed window limiter — مثال ساده برای endpoint های حساس
    options.AddFixedWindowLimiter("fixed-login" , opt =>
    {
        opt.Window = TimeSpan.FromSeconds(60);
        opt.PermitLimit = 5; // حداکثر 5 درخواست در هر 60 ثانیه
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });

    // 3) Partitioned limiter — مثال: محدودسازی به ازای هر IP یا API key
    options.AddPolicy<string>("per-ip" , context =>
    {
        // کلید را از IP استخراج می کنیم (می توانید از header مثل X-Api-Key نیز استفاده کنید)
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetTokenBucketLimiter(ip , _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = 10 ,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst ,
            QueueLimit = 5 ,
            ReplenishmentPeriod = TimeSpan.FromSeconds(1) ,
            TokensPerPeriod = 2 ,
            AutoReplenishment = true
        });
    });

    // 4) Sliding window per API key example
    options.AddPolicy("per-api-key" , context =>
    {
        var apiKey = context.Request.Headers["X-Api-Key"].FirstOrDefault() ?? context.Connection.RemoteIpAddress?.ToString() ?? "anon";
        return RateLimitPartition.GetSlidingWindowLimiter(apiKey , _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 100 ,
            Window = TimeSpan.FromMinutes(1) ,
            SegmentsPerWindow = 6 , // دقت بازه لغزان
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst ,
            QueueLimit = 0
        });
    });

    // Custom rejection handler (برای همه محدودکننده ها)
    options.OnRejected = async (context , ct) =>
    {
        // می توانیم هدر Retry-After را بر اساس نوع محدودساز یا اطلاعات داخلی تنظیم کنیم.
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.Headers["Content-Type"] = "application/json";

        // افزودن یک نمونه هدر Retry-After (ثانیه)
        context.HttpContext.Response.Headers["Retry-After"] = "10";

        var payload = new { error = "Too many requests" , code = 429 };
        await context.HttpContext.Response.WriteAsJsonAsync(payload , cancellationToken: ct);
    };
});

var app = builder.Build();

// Use Rate Limiter middleware
app.UseRateLimiter();

// Default endpoint uses global-token limiter
app.MapGet("/" , () => Results.Ok(new { message = "Hello — global token limiter" }))
   .RequireRateLimiting("global-token");

// Login endpoint — very strict (Fixed Window)
app.MapPost("/login" , (LoginRequest req) =>
{
    // fake login
    return Results.Ok(new { message = "logged in (fake)" });
})
.RequireRateLimiting("fixed-login");

// Endpoint with per-ip limiter
app.MapGet("/ip-limited" , (HttpContext ctx) => Results.Ok(new { ip = ctx.Connection.RemoteIpAddress?.ToString() }))
   .RequireRateLimiting("per-ip");

// Endpoint with per-api-key sliding window
app.MapGet("/apikey-data" , (HttpContext ctx) =>
{
    var apiKey = ctx.Request.Headers["X-Api-Key"].FirstOrDefault() ?? "none";
    return Results.Ok(new { message = "data for apikey" , apikey = apiKey });
})
.RequireRateLimiting("per-api-key");

app.Run();

record LoginRequest(string Username , string Password);