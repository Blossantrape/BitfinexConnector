using System.Text.Json;
using System.Text.Json.Serialization;
using BitfinexConnector.Core.Abstractions;
using BitfinexConnector.Core.Models;
using Microsoft.Extensions.Logging;

namespace BitfinexConnector.Infrastructure.Services;

public class TestConnectorService : ITestConnector
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TestConnectorService> _logger;
    private const string BaseUrl = "https://api-pub.bitfinex.com/v2/";
    private readonly JsonSerializerOptions _jsonOptions;

    public TestConnectorService(HttpClient httpClient, ILogger<TestConnectorService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _jsonOptions = new JsonSerializerOptions
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Converters = { new DecimalConverter() }
        };
    }

    public async Task<List<Trade>> GetTradesAsync(string symbol, int limit)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(symbol))
                throw new ArgumentException("Symbol не может быть пустым", nameof(symbol));

            var validatedSymbol = ValidateSymbol(symbol);
            var url = $"{BaseUrl}trades/t{validatedSymbol}/hist?limit={limit}";

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
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Неверный символ: {Symbol}", symbol);
            return new List<Trade>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении трейдов для {Symbol}", symbol);
            return new List<Trade>();
        }
    }

    public async Task<List<Candle>> GetCandlesAsync(string symbol, string timeFrame, int limit)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении свечей для {Symbol}", symbol);
            return new List<Candle>();
        }
    }

    public async Task<Ticker?> GetTickerAsync(string symbol)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(symbol))
                throw new ArgumentException("Symbol не может быть пустым", nameof(symbol));

            var validatedSymbol = ValidateSymbol(symbol);
            var url = $"{BaseUrl}ticker/t{validatedSymbol}";

            using var response = await _httpClient.GetAsync(url);
        
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Не удалось получить тикер для {Symbol}, код: {StatusCode}", symbol, response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<List<JsonElement>>(json, _jsonOptions);

            if (data == null || data.Count < 10)
                return null;

            return new Ticker
            {
                Symbol = symbol,
                LastPrice = data[6].GetDecimal(),
                Volume = data[7].GetDecimal()
            };
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Неверный символ: {Symbol}", symbol);
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
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol не может быть пустым", nameof(symbol));

        var cleanedSymbol = symbol
            .Replace(" ", "")
            .Replace("-", "") // Удаляем дефисы
            .ToUpper()
            .Replace("USDT", "USD");

        if (cleanedSymbol == "DASHUSD")
            cleanedSymbol = "DSHUSD";

        if (!cleanedSymbol.EndsWith("USD"))
            throw new ArgumentException($"Неверный формат символа: {symbol}");

        var baseCurrency = cleanedSymbol.Substring(0, cleanedSymbol.Length - 3);
        if (baseCurrency.Length < 3)
            throw new ArgumentException($"Неверный формат символа: {symbol}");

        return cleanedSymbol;
    }
}