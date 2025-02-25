using BitfinexConnector.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace BitfinexConnector.Controllers;

public class BitfinexWebSocketController : ControllerBase
{
    private readonly BitfinexWebSocketClient _webSocketClient;

    public BitfinexWebSocketController(BitfinexWebSocketClient webSocketClient)
    {
        _webSocketClient = webSocketClient;
    }

    /// <summary>
    /// Подключает WebSocket клиент.
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> Connect()
    {
        await _webSocketClient.ConnectAsync();
        return Ok("WebSocket подключен");
    }
    
    /// <summary>
    /// Подписка на обновление тикера.
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    public async Task<IActionResult> SubscribeToTicker(string symbol)
    {
        await _webSocketClient.SubscribeToTickerAsync(symbol);
        return Ok($"Подписка на тикер {symbol} установлена");
    }
}