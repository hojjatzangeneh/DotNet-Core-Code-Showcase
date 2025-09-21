
using Confluent.Kafka;
using Confluent.Kafka.Admin;

using KafkaApp.Kafka;
using KafkaApp.Models;

using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

using System.Reflection;

using System.Text.Json;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ===== خدمات و تنظیمات =====
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services
    .AddSwaggerGen(
        c =>
        {
            c.SwaggerDoc(
                "v1",
                new OpenApiInfo
                    {
                        Title = "Kafka Demo API",
                        Version = "v1",
                        Description = "Producer / Consumer / Admin for Kafka (NET 9)"
                    });
            string xml = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xml);
            if(File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath, true);
            }
        });

// Kestrel را مجبور کن روی 5000 گوش بده
builder.WebHost.UseUrls("http://localhost:5000");

// Producer (Singleton)
builder.Services
    .AddSingleton<IProducer<string, string>>(
        sp =>
        {
            KafkaSettings s = sp.GetRequiredService<IOptions<KafkaSettings>>().Value;
            ProducerConfig cfg = new ProducerConfig
            {
                BootstrapServers = s.BootstrapServers,
                Acks = (s.Acks?.ToLower() == "all") ? Acks.All : Acks.Leader,
                EnableIdempotence = s.EnableIdempotence,
                CompressionType =
                    s.Compression?.ToLower() switch
                        {
                            "snappy" => CompressionType.Snappy,
                            "gzip" => CompressionType.Gzip,
                            "lz4" => CompressionType.Lz4,
                            "zstd" => CompressionType.Zstd,
                            _ => CompressionType.None
                        },
                LingerMs = s.LingerMs,
                BatchSize = s.BatchSizeBytes,
                MessageTimeoutMs = 30000,
                ClientId = "kafka-net9-demo-producer"
            };
            return new ProducerBuilder<string, string>(cfg)
        .SetErrorHandler((_, e) => Console.WriteLine($"[PRODUCER ERROR] {e.Reason}"))
                .Build();
        });

// Buffer + Consumer
builder.Services.AddSingleton<MessageBuffer>();
builder.Services.AddHostedService<ConsumerWorker>();

WebApplication app = builder.Build();

// Swagger همیشه فعال
app.UseSwagger();
app.UseSwaggerUI(
    c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Kafka Demo API v1");
        c.RoutePrefix = "swagger"; // UI at /swagger
    });

// Health
app.MapHealthChecks("/health/live").WithTags("Health");
app.MapHealthChecks("/health/ready").WithTags("Health");

// اطمینان از وجود تاپیک پیش‌فرض در شروع
await EnsureDefaultTopicAsync(app.Configuration);

// Producer flush on shutdown
IProducer<string, string> producer = app.Services.GetRequiredService<IProducer<string, string>>();
app.Lifetime.ApplicationStopping.Register(() => producer.Flush(TimeSpan.FromSeconds(5)));

// ===== API =====
RouteGroupBuilder api = app.MapGroup("/api").WithTags("Kafka");

api.MapGet(
    "/topics",
    (IConfiguration cfg) =>
    {
        string bs = cfg.GetSection("Kafka")["BootstrapServers"]!;
        using IAdminClient admin = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = bs }).Build();
        Metadata md = admin.GetMetadata(TimeSpan.FromSeconds(5));
        return Results.Ok(
            md.Topics.Select(t => new { name = t.Topic, partitions = t.Partitions.Count, error = t.Error?.Reason }));
    })
    .WithName("ListTopics")
    .WithSummary("Get topics");

api.MapPost(
    "/topics",
    async (CreateTopicRequest req, IConfiguration cfg) =>
    {
        if(string.IsNullOrWhiteSpace(req.Name))
        {
            return Results.BadRequest(new { error = "Topic name is required" });
        }

        string bs = cfg.GetSection("Kafka")["BootstrapServers"]!;
        using IAdminClient admin = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = bs }).Build();
        TopicSpecification spec = new TopicSpecification
        {
            Name = req.Name,
            NumPartitions = Math.Max(1, req.Partitions),
            ReplicationFactor = (short) Math.Max(1, req.ReplicationFactor)
        };

        try
        {
            await admin.CreateTopicsAsync(new[] { spec });
            return Results.Ok(new { created = req.Name });
        }
    catch(CreateTopicsException ex) when ( ex.Results.Any(r => r.Error.Code == ErrorCode.TopicAlreadyExists) )
        {
            return Results.Conflict(new { error = "Topic already exists" });
        }
    })
    .WithName("CreateTopic");

api.MapPost(
    "/produce",
    async (ProduceRequest req, IProducer<string, string> p, IConfiguration cfg) =>
    {
        string topic = (!string.IsNullOrWhiteSpace(req.Topic))
            ? (req.Topic!)
            : (cfg.GetSection("Kafka")["DefaultTopic"] ?? "test");
        if(string.IsNullOrWhiteSpace(topic))
        {
            return Results.BadRequest(new { error = "Topic is required" });
        }

        string value = req.Value switch
        {
            null => string.Empty,
            string s => s,
            _ => JsonSerializer.Serialize(req.Value)
        };
        try
        {
            DeliveryResult<string, string> dr = await p.ProduceAsync(
                topic,
                new Message<string, string>
                {
                    Key = req.Key,
                    Value = value,
                    Headers = req.Headers?.ToHeaders()
                });
            return Results.Ok(new { topic, partition = dr.Partition.Value, offset = dr.Offset.Value });
        }
    catch(ProduceException<string, string> ex)
        {
            return Results.Problem(title: "Produce failed", detail: ex.Error.Reason);
        }
    })
    .WithName("Produce");

api.MapGet("/messages", (MessageBuffer buf, int? take) => Results.Ok(buf.Snapshot(take ?? 100)))
    .WithName("RecentMessages");

// Root ping + راهنما
app.MapGet(
    "/",
    () => Results.Ok(
        new
        {
            service = ".NET 9 Kafka Demo",
            docs = "/swagger",
            endpoints = new[]
            {
                "GET /api/topics",
                "POST /api/topics",
                "POST /api/produce",
                "GET /api/messages?take=100",
                "GET /health/live",
                "GET /health/ready"
            }
        }));

// لاگ URLها
app.Logger.LogInformation("Listening on: {urls}", string.Join(", ", app.Urls));

await app.RunAsync();

static async Task EnsureDefaultTopicAsync(IConfiguration cfg)
{
    string bootstrap = cfg.GetSection("Kafka")["BootstrapServers"] ?? "localhost:9092";
    string? defaultTopic = cfg.GetSection("Kafka")["DefaultTopic"];
    if(string.IsNullOrWhiteSpace(defaultTopic))
    {
        return;
    }

    using IAdminClient admin = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = bootstrap }).Build();
    try
    {
        Metadata md = admin.GetMetadata(defaultTopic, TimeSpan.FromSeconds(3));
        bool exists = md.Topics.Any(t => (t.Topic == defaultTopic) && !(t.Error?.IsError ?? false));
        if(!exists)
        {
            await admin.CreateTopicsAsync(
                new[]
                {
                    new TopicSpecification { Name = defaultTopic, NumPartitions = 3, ReplicationFactor = 1 }
                });
            Console.WriteLine($"[BOOT] Topic '{defaultTopic}' created.");
        }
    }
    catch(CreateTopicsException ex) when ( ex.Results.Any(r => r.Error.Code == ErrorCode.TopicAlreadyExists) )
    { /* ok */
    }
    catch(Exception e)
    {
        Console.WriteLine($"[BOOT] Ensure topic failed: {e.Message}");
    }
}