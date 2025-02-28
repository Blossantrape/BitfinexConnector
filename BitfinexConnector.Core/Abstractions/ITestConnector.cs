using BitfinexConnector.Core.Models;

namespace BitfinexConnector.Core.Abstractions;

/// <summary>
/// Интерфейс для коннектора, который получает данные с биржи.
/// </summary>
public interface ITestConnector
{
    /// <summary>
    /// Получает список трейдов для заданной валютной пары.
    /// </summary>
    /// <param name="symbol">Торговый инструмент (пример: "BTCUSD").</param>
    /// <param name="limit">Количество записей.</param>
    /// <returns></returns>
    Task<List<Trade>> GetTradesAsync(string symbol, int limit);

    /// <summary>
    /// Получает список свечей для заданной валютной пары и таймфрейма.
    /// </summary>
    /// <param name="symbol">Торговый инструмент (пример: "BTCUSD").</param>
    /// <param name="timeFrame">Таймфрейм (пример: "1m", "5m", "1h").</param>
    /// <param name="limit">Количество записей.</param>
    /// <returns></returns>
    Task<List<Candle>> GetCandlesAsync(string symbol, string timeFrame, int limit);

    /// <summary>
    /// Получает текущую информацию о тикере.
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    Task<Ticker?> GetTickerAsync(string symbol);
}