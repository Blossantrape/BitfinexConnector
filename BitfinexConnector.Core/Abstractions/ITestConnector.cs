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
    /// <param name="symbol"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    Task<List<Trade>> GetTradesAsync(string symbol, int limit = 50);

    /// <summary>
    /// Получает список свечей для заданной валютной пары и таймфрейма.
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="timeFrame"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    Task<List<Candle>> GetCandlesAsync(string symbol, string timeFrame, int limit = 50);

    /// <summary>
    /// Получает текущую информацию о тикере.
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    Task<Ticker?> GetTickerAsync(string symbol);
}