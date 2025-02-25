using BitfinexConnector.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace BitfinexConnector.Controllers;

/// <summary>
/// Контроллер для работы с данными Bitfinex
/// </summary>
[ApiController]
[Route("api/bitfinex")]
public class BitfinexController : ControllerBase
{
    private readonly BitfinexRestClient _bitfinexClient;

    public BitfinexController(BitfinexRestClient bitfinexClient)
    {
        _bitfinexClient = bitfinexClient;
    }

    /// <summary>
    /// Получает последние трейды
    /// </summary>
    [HttpGet("trades/{symbol}")]
    public async Task<IActionResult> GetTrades(string symbol, int limit = 50)
    {
        var trades = await _bitfinexClient.GetTradesAsync(symbol, limit);
        return Ok(trades);
    }

    /// <summary>
    /// Получает свечи для валютной пары
    /// </summary>
    [HttpGet("candles/{symbol}/{timeFrame}")]
    public async Task<IActionResult> GetCandles(string symbol, string timeFrame, int limit = 50)
    {
        var candles = await _bitfinexClient.GetCandlesAsync(symbol, timeFrame, limit);
        return Ok(candles);
    }

    /// <summary>
    /// Получает данные тикера
    /// </summary>
    [HttpGet("ticker/{symbol}")]
    public async Task<IActionResult> GetTicker(string symbol)
    {
        var ticker = await _bitfinexClient.GetTickerAsync(symbol);
        return ticker != null ? Ok(ticker) : NotFound();
    }
}