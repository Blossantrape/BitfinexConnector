using BitfinexConnector.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace BitfinexConnector.Controllers
{
    /// <summary>
    /// Контроллер для тестирования подключения и подписки через WebSocket клиент к бирже Bitfinex.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WebSocketController : ControllerBase
    {
        private readonly WebSocketClientService _wsClient;

        public WebSocketController(WebSocketClientService wsClient)
        {
            _wsClient = wsClient;
        }

        /// <summary>
        /// Подключается к WebSocket API и подписывается на обновления тикера для указанного символа.
        /// Пример: POST /api/websocket/subscribe/ticker/BTCUSD
        /// </summary>
        /// <param name="symbol">Символ валютной пары.</param>
        [HttpPost("subscribe/ticker/{symbol}")]
        public async Task<IActionResult> SubscribeTicker(string symbol)
        {
            await _wsClient.ConnectAsync();
            await _wsClient.SubscribeToTickerAsync(symbol);
            return Ok($"Подписка на обновления тикера для {symbol} выполнена.");
        }

        /// <summary>
        /// Подключается к WebSocket API и подписывается на поток трейдов для указанного символа.
        /// Пример: POST /api/websocket/subscribe/trades/BTCUSD
        /// </summary>
        /// <param name="symbol">Символ валютной пары.</param>
        [HttpPost("subscribe/trades/{symbol}")]
        public async Task<IActionResult> SubscribeTrades(string symbol)
        {
            await _wsClient.ConnectAsync();
            await _wsClient.SubscribeToTradesAsync(symbol);
            return Ok($"Подписка на поток трейдов для {symbol} выполнена.");
        }

        /// <summary>
        /// Подключается к WebSocket API и подписывается на поток свечей для указанного символа и таймфрейма.
        /// Пример: POST /api/websocket/subscribe/candles/BTCUSD/1m
        /// </summary>
        /// <param name="symbol">Символ валютной пары.</param>\n        /// <param name="timeframe">Таймфрейм (например, 1m, 5m, 1h).</param>
        [HttpPost("subscribe/candles/{symbol}/{timeframe}")]
        public async Task<IActionResult> SubscribeCandles(string symbol, string timeframe)
        {
            await _wsClient.ConnectAsync();
            await _wsClient.SubscribeToCandlesAsync(symbol, timeframe);
            return Ok($"Подписка на поток свечей для {symbol} с таймфреймом {timeframe} выполнена.");
        }
    }
}
