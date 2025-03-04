using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using BitfinexConnector.Core.Models;
using Microsoft.Extensions.Logging;

namespace BitfinexConnector.Infrastructure.Services
{
    /// <summary>
    /// Клиент для взаимодействия с WebSocket API Bitfinex.
    /// Реализует подписку и получение обновлений по тикеру, трейдам и свечам.
    /// </summary>
    public class WebSocketClientService : IAsyncDisposable
    {
        private ClientWebSocket _webSocket;
        private readonly ILogger<WebSocketClientService> _logger;
        private readonly CancellationTokenSource _cts;

        /// <summary> Событие обновления тикера. </summary>
        public event Func<Ticker, Task>? OnTickerUpdate;

        /// <summary> Событие получения нового трейда. </summary>
        public event Func<Trade, Task>? OnTradeReceived;

        /// <summary> Событие получения новой свечи. </summary>
        public event Func<Candle, Task>? OnCandleReceived;

        private const string WebSocketUrl = "wss://api-pub.bitfinex.com/ws/2";

        public WebSocketClientService(ILogger<WebSocketClientService> logger)
        {
            _webSocket = new ClientWebSocket();
            _logger = logger;
            _cts = new CancellationTokenSource();
        }

        /// <summary>
        /// Подключается к WebSocket API Bitfinex.
        /// Если соединение уже установлено, повторное подключение не выполняется.
        /// После подключения запускается асинхронный цикл чтения сообщений.
        /// </summary>
        public async Task ConnectAsync()
        {
            if (_webSocket.State == WebSocketState.Open)
                return;

            _logger.LogInformation("Подключение к WebSocket API Bitfinex...");
            await _webSocket.ConnectAsync(new Uri(WebSocketUrl), _cts.Token);
            _logger.LogInformation("Подключено к WebSocket API");

            // Запуск цикла чтения сообщений с передачей CancellationToken.
            _ = Task.Run(() => ReceiveMessagesAsync(), _cts.Token);
        }

        /// <summary>
        /// Отправляет запрос на подписку на обновления тикера для указанного символа.
        /// </summary>
        /// <param name="symbol">Символ валютной пары (например, BTCUSD).</param>
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
        /// Отправляет запрос на подписку на поток трейдов для указанного символа.
        /// </summary>
        /// <param name="symbol">Символ валютной пары (например, BTCUSD).</param>
        public async Task SubscribeToTradesAsync(string symbol)
        {
            var message = new
            {
                @event = "subscribe",
                channel = "trades",
                symbol = $"t{symbol.ToUpper()}"
            };

            await SendMessageAsync(message);
        }

        /// <summary>
        /// Отправляет запрос на подписку на поток свечей для указанного символа и таймфрейма.
        /// </summary>
        /// <param name="symbol">Символ валютной пары (например, BTCUSD).</param>
        /// <param name="timeframe">Таймфрейм (например, 1m, 5m, 1h).</param>
        public async Task SubscribeToCandlesAsync(string symbol, string timeframe)
        {
            var message = new
            {
                @event = "subscribe",
                channel = "candles",
                key = $"trade:{timeframe}:t{symbol.ToUpper()}"
            };

            await SendMessageAsync(message);
        }

        /// <summary>
        /// Асинхронно получает входящие сообщения из WebSocket.
        /// Использует MemoryStream для сборки фрагментированных сообщений.
        /// При получении полного сообщения происходит его обработка.
        /// Если тип сообщения "Close" – инициируется переподключение.
        /// </summary>
        private async Task ReceiveMessagesAsync()
        {
            var buffer = new ArraySegment<byte>(new byte[4096]);

            while (_webSocket.State == WebSocketState.Open)
            {
                using var ms = new MemoryStream();
                WebSocketReceiveResult result;
                do
                {
                    result = await _webSocket.ReceiveAsync(buffer, _cts.Token);
                    ms.Write(buffer.Array!, buffer.Offset, result.Count);
                } while (!result.EndOfMessage);

                var json = Encoding.UTF8.GetString(ms.ToArray());

                // Если сообщение не начинается с '[', считаем его системным и пропускаем.
                if (!json.StartsWith("["))
                    continue;

                try
                {
                    var array = JsonSerializer.Deserialize<List<object>>(json);
                    ProcessMessage(array);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка обработки WebSocket сообщения");
                }

                if (result.MessageType == WebSocketMessageType.Close)
                    await ReconnectAsync();
            }
        }

        /// <summary>
        /// Обрабатывает входящее сообщение, определяя тип данных (трейд, свеча или тикер)
        /// по количеству элементов во вложенном массиве, и вызывает соответствующее событие.
        /// Поддерживает как одиночное обновление, так и снимок (snapshot) – массив массивов.
        /// </summary>
        /// <param name="array">Десериализованный массив объектов из JSON-сообщения.</param>
        private void ProcessMessage(List<object>? array)
        {
            if (array is { Count: > 1 } && array[1] is JsonElement elem && elem.ValueKind == JsonValueKind.Array)
            {
                // Получаем вложенный массив данных
                var dataElements = elem.EnumerateArray().ToArray();

                // Если первый элемент сам является массивом, то это снимок (snapshot) – обрабатываем каждый элемент отдельно.
                if (dataElements.Length > 0 && dataElements[0].ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in dataElements)
                    {
                        ProcessDataElement(item);
                    }
                }
                else
                {
                    // Иначе, обрабатываем одиночное обновление.
                    ProcessDataElement(elem);
                }
            }
        }

        /// <summary>
        /// Обрабатывает один элемент данных, который является массивом значений.
        /// Определяет тип сообщения по количеству элементов и вызывает соответствующее событие.
        /// </summary>
        /// <param name="element">JsonElement, содержащий массив данных.</param>
        private void ProcessDataElement(JsonElement element)
        {
            var data = element.EnumerateArray().ToArray();

            if (data.Length == 4)
            {
                var trade = new Trade
                {
                    Id = data[0].GetInt64(),
                    Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(data[1].GetInt64()).UtcDateTime,
                    Amount = data[2].GetDecimal(),
                    Price = data[3].GetDecimal()
                };
                _ = OnTradeReceived?.Invoke(trade); // Корректный вызов
            }
            else if (data.Length == 6)
            {
                var candle = new Candle
                {
                    Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(data[0].GetInt64()).UtcDateTime,
                    Open = data[1].GetDecimal(),
                    Close = data[2].GetDecimal(),
                    High = data[3].GetDecimal(),
                    Low = data[4].GetDecimal(),
                    Volume = data[5].GetDecimal()
                };
                _ = OnCandleReceived?.Invoke(candle); // Корректный вызов
            }
            else if (data.Length >= 10)
            {
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
                _ = OnTickerUpdate?.Invoke(ticker); // Корректный вызов
            }
            else
            {
                _logger.LogWarning("Получено некорректное сообщение от Bitfinex: {Json}", element.ToString());
            }
        }

        /// <summary>
        /// Сериализует и отправляет сообщение через WebSocket.
        /// </summary>
        /// <param name="message">Объект, который будет сериализован в JSON.</param>
        private async Task SendMessageAsync(object message)
        {
            var json = JsonSerializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(json);
            await _webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, _cts.Token);
        }

        /// <summary>
        /// Выполняет переподключение в случае разрыва соединения.
        /// Производится попытка переподключения с интервалом 5 секунд до успешного восстановления соединения.
        /// </summary>
        private async Task ReconnectAsync()
        {
            _logger.LogWarning("Потеряно соединение с WebSocket, переподключение...");

            while (_webSocket.State != WebSocketState.Open)
            {
                try
                {
                    _webSocket.Dispose();
                    _webSocket = new ClientWebSocket();
                    await ConnectAsync();
                    _logger.LogInformation("Подключение восстановлено.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка переподключения. Повторная попытка через 5 секунд.");
                    await Task.Delay(5000);
                }
            }
        }

        /// <summary>
        /// Асинхронно освобождает ресурсы WebSocket при завершении работы.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Закрытие соединения",
                CancellationToken.None);
            _webSocket.Dispose();
        }
    }
}