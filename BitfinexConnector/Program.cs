using BitfinexConnector.Core.Abstractions;
using BitfinexConnector.Infrastructure.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddHttpClient<ITestConnector, RestClientService>();

// DI.
builder.Services.AddScoped<RestClientService>();
builder.Services.AddSingleton<WebSocketClientService>();
builder.Services.AddSingleton<CacheService>();
builder.Services.AddScoped<PortfolioCalculator>();

// Настраиваем Swagger для тестирования API.
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

// Включаем Swagger в режиме разработки.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();