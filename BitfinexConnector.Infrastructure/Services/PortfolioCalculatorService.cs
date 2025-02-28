using BitfinexConnector.Core.Abstractions;
using BitfinexConnector.Core.Models;
using Microsoft.Extensions.Logging;

namespace BitfinexConnector.Infrastructure.Services
{
    /// <summary>
    /// Класс для расчёта общего баланса портфеля в разных валютах.
    /// Балансы по каждой криптовалюте конвертируются в USDT, суммируются,
    /// а затем общее значение конвертируется обратно в каждую валюту.
    /// </summary>
    public class PortfolioCalculatorService
    {
        private readonly ITestConnector _connector;
        private readonly ILogger _logger;

        public PortfolioCalculatorService(ITestConnector connector, ILogger<PortfolioCalculatorService> logger)
        {
            _connector = connector;
            _logger = logger;
        }

        /// <summary>
        /// Рассчитывает общий баланс портфеля в USDT, BTC, XRP, XMR и DASH.
        /// Балансы задаются через словарь, где ключ – символ криптовалюты, значение – баланс.
        /// </summary>
        /// <param name="balances">Словарь балансов (например, { "BTC", 1 }, { "XRP", 15000 } и т.д.).</param>
        /// <returns>Словарь, где ключ – валюта (USDT, BTC, XRP, XMR, DASH), а значение – рассчитанный баланс.</returns>
        public async Task<Dictionary<string, decimal>> CalculatePortfolioAsync(Dictionary<string, decimal> balances)
        {
            // Получаем тикеры для конвертации в USDT.
            Ticker? btcTicker = await _connector.GetTickerAsync("BTCUSDT");
            Ticker? xrpTicker = await _connector.GetTickerAsync("XRPUSDT");
            Ticker? xmrTicker = await _connector.GetTickerAsync("XMRUSDT");
            Ticker? dashTicker = await _connector.GetTickerAsync("DASHUSDT");

            // Логируем, какие тикеры не были получены
            if (btcTicker == null)
                _logger.LogError("Не удалось получить тикер для BTCUSDT");
            if (xrpTicker == null)
                _logger.LogError("Не удалось получить тикер для XRPUSDT");
            if (xmrTicker == null)
                _logger.LogError("Не удалось получить тикер для XMRUSDT");
            if (dashTicker == null)
                _logger.LogError("Не удалось получить тикер для DASHUSDT");

            // Если хотя бы один тикер не получен, выбрасываем исключение
            if (btcTicker == null || xrpTicker == null || xmrTicker == null || dashTicker == null)
            {
                throw new Exception("Не удалось получить данные тикера для одной или нескольких валют.");
            }

            // Рассчитываем общую стоимость портфеля в USDT.
            decimal totalUSDT = 0;
            if (balances.TryGetValue("BTC", out decimal btcBalance))
                totalUSDT += btcBalance * btcTicker.LastPrice;
            if (balances.TryGetValue("XRP", out decimal xrpBalance))
                totalUSDT += xrpBalance * xrpTicker.LastPrice;
            if (balances.TryGetValue("XMR", out decimal xmrBalance))
                totalUSDT += xmrBalance * xmrTicker.LastPrice;
            if (balances.TryGetValue("DASH", out decimal dashBalance))
                totalUSDT += dashBalance * dashTicker.LastPrice;

            // Конвертируем общую стоимость обратно в каждую валюту.
            var result = new Dictionary<string, decimal>
            {
                ["USDT"] = totalUSDT,
                ["BTC"] = btcTicker.LastPrice != 0 ? totalUSDT / btcTicker.LastPrice : 0,
                ["XRP"] = xrpTicker.LastPrice != 0 ? totalUSDT / xrpTicker.LastPrice : 0,
                ["XMR"] = xmrTicker.LastPrice != 0 ? totalUSDT / xmrTicker.LastPrice : 0,
                ["DASH"] = dashTicker.LastPrice != 0 ? totalUSDT / dashTicker.LastPrice : 0
            };

            return result;
        }
    }
}