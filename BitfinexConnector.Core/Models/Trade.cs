namespace BitfinexConnector.Core.Models;

/// <summary>
/// Модель трейда (сделки) на бирже.
/// </summary>
public class Trade
{
    /// <summary>
    /// Уникальный идентификатор трейда.
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// Время сделки.
    /// </summary>
    public DateTime Timestamp { get; set; } 
    
    /// <summary>
    /// Символ валютной пары.
    /// </summary>
    public string Symbol { get; set; } = string.Empty;
    
    /// <summary>
    /// Цена сделки.
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Количество валюты в сделке.
    /// </summary>
    public decimal Amount { get; set; }
}