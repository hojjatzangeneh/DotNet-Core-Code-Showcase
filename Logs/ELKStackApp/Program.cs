using Elastic.Clients.Elasticsearch;

using ELKStackApp.Models;
using ELKStackApp.Services;

using Microsoft.OpenApi.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
Uri node = new Uri("http://localhost:9200");

// If you need authentication
ElasticsearchClientSettings settings = new ElasticsearchClientSettings(node)
    // .Authentication(new BasicAuthentication("elastic" , "your_password"))
    .DefaultIndex("esdemo"); // optional

ElasticsearchClient client = new ElasticsearchClient(settings);

builder.Services.AddSingleton<ElasticsearchClient>(client);
builder.Services.AddScoped<IElasticSearchService<MyDocument>, ElasticSearchService<MyDocument>>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services
    .AddSwaggerGen(
        c => c.SwaggerDoc(
            "v1",
            new OpenApiInfo
            {
                Title = "My API",
                Version = "v1"
            }));
WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();                 // generates swagger.json
    app.UseSwaggerUI(
        c =>             // swagger UI at /swagger
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            c.RoutePrefix = string.Empty; // host UI at root (/)
        });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();