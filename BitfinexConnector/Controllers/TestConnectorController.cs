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
    public class TestConnectorController : ControllerBase
    {
        private readonly ITestConnector _connector;

        public TestConnectorController(ITestConnector connector)
        {
            _connector = connector;
        }

        /// <summary>
        /// Получает последние сделки для указанного символа.
        /// Пример: GET /api/market/trades/BTCUSD?limit=100
        /// </summary>
        /// <param name="symbol">Символ валютной пары.</param>
        /// <param name="limit">Количество записей.</param>
        /// <returns>Список сделок (trades).</returns>
        [HttpGet("trades/{symbol}")]
        public async Task<ActionResult<List<Trade>>> GetTrades(string symbol, [FromQuery] int limit = 100)
        {
            var trades = await _connector.GetTradesAsync(symbol, limit);
            return Ok(trades);
        }

        /// <summary>
        /// Получает свечи для указанного символа и таймфрейма.
        /// Пример: GET /api/market/candles/BTCUSD/1m?limit=100
        /// </summary>
        /// <param name="symbol">Символ валютной пары.</param>
        /// <param name="timeframe">Таймфрейм (например, 1m, 5m, 1h).</param>
        /// <param name="limit">Количество записей.</param>
        /// <returns>Список свечей.</returns>
        [HttpGet("candles/{symbol}/{timeframe}")]
        public async Task<ActionResult<List<Candle>>> GetCandles(string symbol, string timeframe,
            [FromQuery] int limit = 100)
        {
            var candles = await _connector.GetCandlesAsync(symbol, timeframe, limit);
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