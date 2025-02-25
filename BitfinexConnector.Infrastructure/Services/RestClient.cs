using System.Net.Http.Json;using BitfinexConnector.Core.Abstractions;
using BitfinexConnector.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BitfinexConnector.Infrastructure.Services;

/// <summary>
/// Клиент для взаимодействия с REST API Bitfinex.
/// </summary>
public class RestClient : ITestConnector
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RestClient> _logger;

    private const string BaseUrl = "https://api-pub.bitfinex.com/v2/";

    public RestClient(HttpClient httpClient, ILogger<RestClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Получает список трейдов для валютной пары.
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    public async Task<List<Trade>> GetTradesAsync(string symbol, int limit = 50)
    {
        var url = $"{BaseUrl}trades/t{symbol.ToUpper()}/hist?limit={limit}";
        try
        {
            var response = await _httpClient.GetStringAsync(url);
            var data = JsonConvert.DeserializeObject<List<List<object>>>(response);

            return data?.Select(trade => new Trade
            {
                Id = Convert.ToInt64(trade[0]),
                Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(trade[1])).UtcDateTime,
                Price = Convert.ToDecimal(trade[3]),
                Amount = Convert.ToDecimal(trade[2]),
                Symbol = symbol
            }).ToList() ?? new List<Trade>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении трейдов для {Symbol}", symbol);
            return new List<Trade>();
        }
    }

    /// <summary>
    /// Получает список трейдов для валютной пары.
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="timeFrame"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    public async Task<List<Candle>> GetCandlesAsync(string symbol, string timeFrame, int limit = 50)
    {
        var url = $"{BaseUrl}candles/trade:{timeFrame}:t{symbol.ToUpper()}/hist?limit={limit}";
        try
        {
            var response = await _httpClient.GetStringAsync(url);
            var data = JsonConvert.DeserializeObject<List<List<object>>>(response);

            return data?.Select(candle => new Candle
            {
                Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(candle[0])).UtcDateTime,
                Open = Convert.ToDecimal(candle[1]),
                Close = Convert.ToDecimal(candle[2]),
                High = Convert.ToDecimal(candle[3]),
                Low = Convert.ToDecimal(candle[4]),
                Volume = Convert.ToDecimal(candle[5])
            }).ToList() ?? new List<Candle>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении свечей для {Symbol}", symbol);
            return new List<Candle>();
        }
    }

    /// <summary>
    /// Получает данные о тикере валютной пары.
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    public async Task<Ticker?> GetTickerAsync(string symbol)
    {
        var url = $"{BaseUrl}ticker/t{symbol.ToUpper()}";
        try
        {
            var response = await _httpClient.GetStringAsync(url);
            var data = JsonConvert.DeserializeObject<List<object>>(response);

            if (data == null || data.Count < 10)
                return null;

            return new Ticker
            {
                Symbol = symbol,
                LastPrice = Convert.ToDecimal(data[6]),
                DailyChange = Convert.ToDecimal(data[4]),
                DailyChangePercent = Convert.ToDecimal(data[5]),
                Volume = Convert.ToDecimal(data[7]),
                High = Convert.ToDecimal(data[8]),
                Low = Convert.ToDecimal(data[9])
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении тикера для {Symbol}", symbol);
            return null;
        }
    }
}
