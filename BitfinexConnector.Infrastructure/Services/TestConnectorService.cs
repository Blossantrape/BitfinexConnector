using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using BitfinexConnector.Core.Abstractions;
using BitfinexConnector.Core.Models;
using Microsoft.Extensions.Logging;

namespace BitfinexConnector.Infrastructure.Services
{
    public class TestConnectorService : ITestConnector
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TestConnectorService> _logger;
        private const string BaseUrl = "https://api-pub.bitfinex.com/v2/";
        private readonly JsonSerializerOptions _jsonOptions;

        public TestConnectorService(HttpClient httpClient, ILogger<TestConnectorService> logger)
        {
            // Проверка и инициализация зависимостей
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Настройка опций сериализации JSON
            _jsonOptions = new JsonSerializerOptions
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                Converters = { new DecimalConverter() }
            };
        }

        /// <summary>
        /// Получение списка торгов для указанного символа.
        /// Если символ пустой, будет выброшено исключение.
        /// </summary>
        /// <param name="symbol">Торговая пара</param>
        /// <param name="limit">Лимит количества записей</param>
        /// <returns>Список торгов</returns>
        /// <exception cref="ArgumentException">Если символ пустой или некорректный</exception>
        public async Task<List<Trade>> GetTradesAsync(string symbol, int limit)
        {
            try
            {
                // Проверка валидности символа
                if (string.IsNullOrWhiteSpace(symbol))
                    throw new ArgumentException("Symbol не может быть пустым", nameof(symbol));

                var validatedSymbol = ValidateSymbol(symbol);
                var url = $"{BaseUrl}trades/t{validatedSymbol}/hist?limit={limit}";

                // Отправка HTTP-запроса
                using var response = await _httpClient.GetAsync(url);

                // Если ресурс не найден, возвращаем пустой список
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return new List<Trade>();

                // Проверка успешности запроса
                response.EnsureSuccessStatusCode();

                // Сериализация ответа в список торгов
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<List<List<JsonElement>>>(json, _jsonOptions);

                // Преобразование данных в объекты Trade
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
                return new List<Trade>(); // Возвращаем пустой список в случае ошибки
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении трейдов для {Symbol}", symbol);
                return new List<Trade>(); // Возвращаем пустой список в случае ошибки
            }
        }

        /// <summary>
        /// Получение списка свечей для указанного символа и временного интервала.
        /// Если символ пустой, будет выброшено исключение.
        /// </summary>
        /// <param name="symbol">Торговая пара</param>
        /// <param name="timeFrame">Временной интервал для свечей (например, "1m", "5m")</param>
        /// <param name="limit">Лимит количества записей</param>
        /// <returns>Список свечей</returns>
        /// <exception cref="ArgumentException">Если символ или временной интервал пустые или некорректные</exception>
        public async Task<List<Candle>> GetCandlesAsync(string symbol, string timeFrame, int limit)
        {
            // Проверка валидности входных данных
            if (string.IsNullOrWhiteSpace(symbol))
                throw new ArgumentException("Symbol не может быть пустым", nameof(symbol));
            if (string.IsNullOrWhiteSpace(timeFrame))
                throw new ArgumentException("TimeFrame не может быть пустым", nameof(timeFrame));

            var validatedSymbol = ValidateSymbol(symbol);
            var url = $"{BaseUrl}candles/trade:{timeFrame}:t{validatedSymbol}/hist?limit={limit}";

            try
            {
                // Отправка HTTP-запроса
                using var response = await _httpClient.GetAsync(url);

                // Если ресурс не найден, возвращаем пустой список
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return new List<Candle>();

                // Проверка успешности запроса
                response.EnsureSuccessStatusCode();

                // Сериализация ответа в список свечей
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<List<List<JsonElement>>>(json, _jsonOptions);

                // Преобразование данных в объекты Candle
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
                return new List<Candle>(); // Возвращаем пустой список в случае ошибки
            }
        }

        /// <summary>
        /// Получение тикера для указанного символа.
        /// Если символ пустой, будет выброшено исключение.
        /// </summary>
        /// <param name="symbol">Торговая пара</param>
        /// <returns>Информация о тикере</returns>
        /// <exception cref="ArgumentException">Если символ пустой или некорректный</exception>
        public async Task<Ticker?> GetTickerAsync(string symbol)
        {
            try
            {
                // Проверка валидности символа
                if (string.IsNullOrWhiteSpace(symbol))
                    throw new ArgumentException("Symbol не может быть пустым", nameof(symbol));

                var validatedSymbol = ValidateSymbol(symbol);
                var url = $"{BaseUrl}ticker/t{validatedSymbol}";

                // Отправка HTTP-запроса
                using var response = await _httpClient.GetAsync(url);

                // Если запрос не успешен, логируем предупреждение и возвращаем null
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Не удалось получить тикер для {Symbol}, код: {StatusCode}", symbol,
                        response.StatusCode);
                    return null;
                }

                // Сериализация ответа в объект тикера
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<List<JsonElement>>(json, _jsonOptions);

                // Если данных недостаточно для тикера, возвращаем null
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
                return null; // Возвращаем null в случае ошибки
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении тикера для {Symbol}", symbol);
                return null; // Возвращаем null в случае ошибки
            }
        }

        /// <summary>
        /// Валидация символа торговой пары.
        /// Удаляет пробелы и дефисы, заменяет "USDT" на "USD", проверяет корректность формата.
        /// </summary>
        /// <param name="symbol">Торговая пара</param>
        /// <returns>Очистенный и валидный символ</returns>
        /// <exception cref="ArgumentException">Если символ имеет неверный формат</exception>
        private string ValidateSymbol(string symbol)
        {
            // Проверка на пустой символ
            if (string.IsNullOrWhiteSpace(symbol))
                throw new ArgumentException("Symbol не может быть пустым", nameof(symbol));

            // Очистка символа
            var cleanedSymbol = symbol
                .Replace(" ", "")
                .Replace("-", "") // Удаляем дефисы
                .ToUpper()
                .Replace("USDT", "USD");

            // Исправляем специфические символы
            if (cleanedSymbol == "DASHUSD")
                cleanedSymbol = "DSHUSD";

            // Проверка на формат символа
            if (!cleanedSymbol.EndsWith("USD"))
                throw new ArgumentException($"Неверный формат символа: {symbol}");

            var baseCurrency = cleanedSymbol.Substring(0, cleanedSymbol.Length - 3);
            if (baseCurrency.Length < 3)
                throw new ArgumentException($"Неверный формат символа: {symbol}");

            return cleanedSymbol;
        }
    }
}