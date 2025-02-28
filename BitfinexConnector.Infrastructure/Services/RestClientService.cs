using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using BitfinexConnector.Core.Abstractions;
using BitfinexConnector.Core.Models;
using Microsoft.Extensions.Logging;

namespace BitfinexConnector.Infrastructure.Services;

public class RestClientService : ITestConnector
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RestClientService> _logger;
    
    private const string BaseUrl = "https://api-pub.bitfinex.com/v2/";
    private readonly JsonSerializerOptions _jsonOptions;

    public RestClientService(HttpClient httpClient, ILogger<RestClientService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Настройки для безопасного парсинга чисел
        _jsonOptions = new JsonSerializerOptions
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Converters = { new DecimalConverter() }
        };
    }

    public async Task<List<Trade>> GetTradesAsync(string symbol, int limit = 50)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol не может быть пустым", nameof(symbol));

        var validatedSymbol = ValidateSymbol(symbol);
        var url = $"{BaseUrl}trades/t{validatedSymbol}/hist?limit={limit}";

        try
        {
            using var response = await _httpClient.GetAsync(url);
        
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new List<Trade>();

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<List<List<JsonElement>>>(json, _jsonOptions);

            return data?.Select(trade => new Trade
            {
                Id = trade[0].GetInt64(),
                Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(trade[1].GetInt64()).UtcDateTime,
                Price = trade[3].GetDecimal(),
                Amount = trade[2].GetDecimal(),
                Symbol = symbol
            }).ToList() ?? new List<Trade>();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Торговая пара {Symbol} не найдена", symbol);
            return new List<Trade>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении трейдов для {Symbol}", symbol);
            return new List<Trade>();
        }
    }

    public async Task<List<Candle>> GetCandlesAsync(string symbol, string timeFrame, int limit = 50)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol не может быть пустым", nameof(symbol));
        if (string.IsNullOrWhiteSpace(timeFrame))
            throw new ArgumentException("TimeFrame не может быть пустым", nameof(timeFrame));

        var validatedSymbol = ValidateSymbol(symbol);
        var url = $"{BaseUrl}candles/trade:{timeFrame}:t{validatedSymbol}/hist?limit={limit}";

        try
        {
            using var response = await _httpClient.GetAsync(url);
        
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new List<Candle>();

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<List<List<JsonElement>>>(json, _jsonOptions);

            return data?.Select(candle => new Candle
            {
                Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(candle[0].GetInt64()).UtcDateTime,
                Open = candle[1].GetDecimal(),
                Close = candle[2].GetDecimal(),
                High = candle[3].GetDecimal(),
                Low = candle[4].GetDecimal(),
                Volume = candle[5].GetDecimal()
            }).ToList() ?? new List<Candle>();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Данные свечей для {Symbol} не найдены", symbol);
            return new List<Candle>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении свечей для {Symbol}", symbol);
            return new List<Candle>();
        }
    }

    public async Task<Ticker?> GetTickerAsync(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol не может быть пустым", nameof(symbol));

        var validatedSymbol = ValidateSymbol(symbol);
        var url = $"{BaseUrl}ticker/t{validatedSymbol}";

        try
        {
            using var response = await _httpClient.GetAsync(url);
        
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<List<JsonElement>>(json, _jsonOptions);

            if (data == null || data.Count < 10)
                return null;

            return new Ticker
            {
                Symbol = symbol,
                LastPrice = data[6].GetDecimal(),
                DailyChange = data[4].GetDecimal(),
                DailyChangePercent = data[5].GetDecimal(),
                Volume = data[7].GetDecimal(),
                High = data[8].GetDecimal(),
                Low = data[9].GetDecimal()
            };
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Тикер для {Symbol} не найден", symbol);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении тикера для {Symbol}", symbol);
            return null;
        }
    }

    private string ValidateSymbol(string symbol)
    {
        // Bitfinex использует формат BTCUSD вместо BTCUSDT
        var cleanedSymbol = symbol
            .Replace("USDT", "USD")
            .Replace(" ", "")
            .ToUpper();

        if (cleanedSymbol.Length != 6)
            throw new ArgumentException("Неверный формат символа. Ожидается 6 символов (например: BTCUSD)");

        return cleanedSymbol;
    }
}

// Конвертер для безопасного парсинга decimal
public class DecimalConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number => reader.GetDecimal(),
            JsonTokenType.String => decimal.TryParse(reader.GetString(), out var result) 
                ? result 
                : throw new JsonException("Неверный формат decimal"),
            _ => throw new JsonException("Неверный формат decimal")
        };
    }

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}