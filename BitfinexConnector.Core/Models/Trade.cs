namespace BitfinexConnector.Core.Models;

/// <summary>
/// Модель трейда (сделки) на бирже
/// </summary>
public class Trade
{
    public long Id { get; set; } // Уникальный идентификатор трейда
    public DateTime Timestamp { get; set; } // Время сделки
    public string Symbol { get; set; } = string.Empty; // Символ валютной пары
    public decimal Price { get; set; } // Цена сделки
    public decimal Amount { get; set; } // Количество валюты в сделке
}