using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Http.Resilience;

using Polly;
using Polly.Registry;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddResiliencePipeline("myPipeline" , pipeline =>
{
    pipeline.AddRetry(new Polly.Retry.RetryStrategyOptions
    {
        MaxRetryAttempts = 3 ,
        Delay = TimeSpan.FromSeconds(2) ,
    });
    pipeline.AddTimeout(new HttpTimeoutStrategyOptions
    {
        Timeout = TimeSpan.FromSeconds(10)
    });
});
builder.Services.AddHttpClient("MyApiClient" , c => c.BaseAddress = new Uri("https://api.example.com"))
    .AddResilienceHandler("myPipeline" , pipelineBuilder =>
    {
        pipelineBuilder.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 3 ,
            Delay = TimeSpan.FromSeconds(2)
        });
        pipelineBuilder.AddTimeout(new HttpTimeoutStrategyOptions
        {
            Timeout = TimeSpan.FromSeconds(10)
        });
    });

var app = builder.Build();
app.MapGet("/" , async ([FromServices] IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("MyApiClient");
    return await client.GetStringAsync("/data");
});
app.MapGet("/v1" , async ([FromServices] ResiliencePipelineProvider<string> provider) =>
{
    var pipeline = provider.GetPipeline("myPipeline");
    await pipeline.ExecuteAsync(async (ct) =>
    {
        await Task.Delay(1000 , ct);
        return "OK";
    });
});

app.Run();
