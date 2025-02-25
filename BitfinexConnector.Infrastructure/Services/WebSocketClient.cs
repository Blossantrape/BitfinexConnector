using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using BitfinexConnector.Core.Models;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.Logging;

namespace BitfinexConnector.Infrastructure.Services;

/// <summary>
/// Клиент для взаимодействия с WebSocket API Bitfinex.
/// </summary>
public class WebSocketClient : IAsyncDisposable
{
    private readonly ClientWebSocket _webSocket;
    private readonly ILogger<WebSocketClient> _logger;
    private readonly CancellationTokenSource _cts;
    private readonly CacheService _cacheService;

    public event Func<Ticker, Task>? OnTickerUpdate;

    private const string WebSocketUrl = "wss://api-pub.bitfinex.com/ws/2";

    public WebSocketClient(ILogger<WebSocketClient> logger, CacheService cacheService)
    {
        _webSocket = new ClientWebSocket();
        _logger = logger;
        _cts = new CancellationTokenSource();
        _cacheService = cacheService;
    }

    /// <summary>
    /// Подключение к WebSocket серверу Bitfinex.
    /// </summary>
    public async Task ConnectAsync()
    {
        if (_webSocket.State == WebSocketState.Open)
            return;

        _logger.LogInformation("Подключение к WebSocket API Bitfinex...");
        await _webSocket.ConnectAsync(new Uri(WebSocketUrl), _cts.Token);
        _logger.LogInformation("Подключено к WebSocket API");

        _ = Task.Run(ReceiveMessagesAsync);
    }

    /// <summary>
    /// Подписка на обновления тикера.
    /// </summary>
    /// <param name="symbol"></param>
    public async Task SubscribeToTickerAsync(string symbol)
    {
        var message = new
        {
            @event = "subscribe",
            channel = "ticker",
            symbol = $"t{symbol.ToUpper()}"
        };

        await SendMessageAsync(message);
    }

    /// <summary>
    /// Получение и обработка сообщений от WebSocket.
    /// </summary>
    private async Task ReceiveMessagesAsync()
    {
        var buffer = new byte[4096];

        while (_webSocket.State == WebSocketState.Open)
        {
            var result = await _webSocket.ReceiveAsync(buffer, _cts.Token);
            var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

            if (!json.StartsWith("["))
                continue;

            try
            {
                var array = JsonSerializer.Deserialize<List<object>>(json);
                if (array is { Count: > 1 } && array[1] is JsonElement elem && elem.ValueKind == JsonValueKind.Array)
                {
                    var data = elem.EnumerateArray().ToArray();

                    var ticker = new Ticker
                    {
                        Symbol = "Unknown",
                        LastPrice = data[6].GetDecimal(),
                        DailyChange = data[4].GetDecimal(),
                        DailyChangePercent = data[5].GetDecimal(),
                        Volume = data[7].GetDecimal(),
                        High = data[8].GetDecimal(),
                        Low = data[9].GetDecimal()
                    };

                    if (OnTickerUpdate != null)
                        await OnTickerUpdate.Invoke(ticker);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обработки WebSocket сообщения");
            }
        }
    }

    /// <summary>
    /// Отправка сообщения в WebSocket.
    /// </summary>
    /// <param name="message"></param>
    private async Task SendMessageAsync(object message)
    {
        var json = JsonSerializer.Serialize(message);
        var bytes = Encoding.UTF8.GetBytes(json);
        await _webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, _cts.Token);
    }

    /// <summary>
    /// Отключение WebSocket.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        _logger.LogInformation("Отключение от WebSocket API Bitfinex...");
        if (_webSocket.State == WebSocketState.Open)
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);

        _webSocket.Dispose();
    }
}
