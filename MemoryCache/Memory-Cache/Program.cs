
using Memory_Cache.Repositories;

using Memory_Cache.Services;

using System.Text.Json;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Memory cache config
builder.Services
    .AddMemoryCache(
        o =>
        {
            // Size limit unit is logical; we use 1 per product
            long? size = builder.Configuration.GetValue<long?>("Cache:Memory:SizeLimit");
            if(size.HasValue)
            {
                o.SizeLimit = size;
            }
        });

// Distributed cache -> Redis
string redisConn = builder.Configuration.GetValue<string>("Cache:Distributed:RedisConnection") ?? "localhost:6379";
string instance = builder.Configuration.GetValue<string>("Cache:Distributed:InstanceName") ?? "shopcache:";
builder.Services
    .AddStackExchangeRedisCache(
        o =>
        {
            o.Configuration = redisConn;
            o.InstanceName = instance;
        });

// LazyCache
builder.Services.AddLazyCache();
// DI for repo & services
builder.Services.AddSingleton<IProductRepository, ProductRepository>();
builder.Services.AddScoped<MemoryCacheProductService>();
builder.Services.AddScoped<DistributedCacheProductService>();
builder.Services.AddScoped<HybridCacheProductService>();
builder.Services.AddScoped<LazyCacheProductService>();
builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();
app.MapGet("/", handler: () => Results.Redirect("/swagger"));
app.MapGet("/healthz", () => Results.Ok(new { ok = true, ts = DateTimeOffset.UtcNow }));

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();