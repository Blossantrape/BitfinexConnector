using System.Reflection;
using BitfinexConnector.Core.Abstractions;
using BitfinexConnector.Infrastructure.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddMemoryCache();

// Правильная регистрация HttpClient + интерфейса
builder.Services.AddHttpClient<ITestConnector, RestClientService>();

builder.Services.AddSingleton<WebSocketClientService>();
//builder.Services.AddSingleton<CacheService>();
builder.Services.AddScoped<PortfolioCalculator>();

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
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

    // Включение XML-документации
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }