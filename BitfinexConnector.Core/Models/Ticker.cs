namespace BitfinexConnector.Core.Models;

/// <summary>
/// Модель тикера валютной пары.
/// </summary>
public class Ticker
{
    public string Symbol { get; set; } = string.Empty; // Символ валютной пары.
    public decimal LastPrice { get; set; } // Последняя цена.
    public decimal DailyChange { get; set; } // Изменение за сутки.
    public decimal DailyChangePercent { get; set; } // Изменение за сутки в процентах.
    public decimal Volume { get; set; } // Объём торгов.
    public decimal High { get; set; } // Максимальная цена за сутки.
    public decimal Low { get; set; } // Минимальная цена за сутки.
}