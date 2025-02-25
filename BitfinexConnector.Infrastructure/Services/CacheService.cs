using BitfinexConnector.Core.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BitfinexConnector.Infrastructure.Services;

/// <summary>
/// Сервис для кэширования данных с Bitfinex WebSocket API.
/// </summary>
public class CacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CacheService> _logger;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(30);

    public CacheService(IMemoryCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Обновление данных тикера в кэше.
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="ticker"></param>
    public void UpdateTicker(string symbol, Ticker ticker)
    {
        _cache.Set(symbol, ticker, _cacheDuration);
        _logger.LogInformation("Кэш обновлён для {Symbol}: {Price}", symbol, ticker.LastPrice);
    }

    /// <summary>
    /// Получение данных тикера из кэша.
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    public Ticker? GetTicker(string symbol)
    {
        _cache.TryGetValue(symbol, out Ticker? ticker);
        return ticker;
    }
}