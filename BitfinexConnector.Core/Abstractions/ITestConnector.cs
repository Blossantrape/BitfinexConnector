using BitfinexConnector.Core.Models;

namespace BitfinexConnector.Core.Abstractions;

/// <summary>
/// Интерфейс для коннектора, который получает данные с биржи
/// </summary>
public interface ITestConnector
{
    /// <summary>
    /// Получает список трейдов для заданной валютной пары
    /// </summary>
    Task<List<Trade>> GetTradesAsync(string symbol, int limit = 50);

    /// <summary>
    /// Получает список свечей для заданной валютной пары и таймфрейма
    /// </summary>
    Task<List<Candle>> GetCandlesAsync(string symbol, string timeFrame, int limit = 50);

    /// <summary>
    /// Получает текущую информацию о тикере
    /// </summary>
    Task<Ticker?> GetTickerAsync(string symbol);
}