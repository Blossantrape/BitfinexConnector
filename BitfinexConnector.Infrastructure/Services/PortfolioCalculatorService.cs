using BitfinexConnector.Core.Abstractions;
using BitfinexConnector.Core.Models;

namespace BitfinexConnector.Infrastructure.Services
{
    /// <summary>
    /// Класс для расчёта общего баланса портфеля в разных валютах.
    /// Балансы по каждой криптовалюте конвертируются в USDT, суммируются,
    /// а затем общее значение конвертируется обратно в каждую валюту.
    /// </summary>
    public class PortfolioCalculator
    {
        private readonly ITestConnector _connector;

        public PortfolioCalculator(ITestConnector connector)
        {
            _connector = connector;
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
            // В запросе передаем пары с USDT.
            Ticker btcTicker = await _connector.GetTickerAsync("BTCUSDT");
            Ticker xrpTicker = await _connector.GetTickerAsync("XRPUSDT");
            Ticker xmrTicker = await _connector.GetTickerAsync("XMRUSDT");
            Ticker dashTicker = await _connector.GetTickerAsync("DASHUSDT");

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

            // Конвертируем общую стоимость обратно в каждую валюту:
            // Для каждой валюты значение = totalUSDT / (курс валюты к USDT).
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
