using BitfinexConnector.Core.Abstractions;
using BitfinexConnector.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace BitfinexConnector.Controllers
{
    /// <summary>
    /// Контроллер для получения рыночных данных через REST API биржи Bitfinex.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MarketController : ControllerBase
    {
        private readonly ITestConnector _connector;

        public MarketController(ITestConnector connector)
        {
            _connector = connector;
        }

        /// <summary>
        /// Получает последние сделки для указанного символа.
        /// Пример: GET /api/market/trades/BTCUSD
        /// </summary>
        /// <param name="symbol">Символ валютной пары.</param>
        /// <returns>Список сделок (trades).</returns>
        [HttpGet("trades/{symbol}")]
        public async Task<ActionResult<List<Trade>>> GetTrades(string symbol)
        {
            var trades = await _connector.GetTradesAsync(symbol);
            return Ok(trades);
        }

        /// <summary>
        /// Получает свечи для указанного символа и таймфрейма.
        /// Пример: GET /api/market/candles/BTCUSD/1m
        /// </summary>
        /// <param name="symbol">Символ валютной пары.</param>
        /// <param name="timeframe">Таймфрейм (например, 1m, 5m, 1h).</param>
        /// <returns>Список свечей.</returns>
        [HttpGet("candles/{symbol}/{timeframe}")]
        public async Task<ActionResult<List<Candle>>> GetCandles(string symbol, string timeframe)
        {
            var candles = await _connector.GetCandlesAsync(symbol, timeframe);
            return Ok(candles);
        }

        /// <summary>
        /// Получает данные тикера для указанного символа.
        /// Пример: GET /api/market/ticker/BTCUSD
        /// </summary>
        /// <param name="symbol">Символ валютной пары.</param>
        /// <returns>Объект тикера.</returns>
        [HttpGet("ticker/{symbol}")]
        public async Task<ActionResult<Ticker>> GetTicker(string symbol)
        {
            var ticker = await _connector.GetTickerAsync(symbol);
            return Ok(ticker);
        }
    }
}
