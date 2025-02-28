using BitfinexConnector.Core.Abstractions;
using BitfinexConnector.Infrastructure.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

// Правильная регистрация HttpClient + интерфейса
builder.Services.AddHttpClient<ITestConnector, RestClientService>();

builder.Services.AddSingleton<WebSocketClientService>();
builder.Services.AddSingleton<CacheService>();
builder.Services.AddScoped<PortfolioCalculator>();

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:7106")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Bitfinex API Connector",
        Version = "v1",
        Description = "API для получения данных с биржи Bitfinex"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();
app.UseCors();
app.MapControllers();
app.Run();

public partial class Program { }