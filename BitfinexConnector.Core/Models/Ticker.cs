namespace BitfinexConnector.Core.Models;

/// <summary>
/// Модель тикера валютной пары.
/// </summary>
public class Ticker
{
    /// <summary>
    /// Символ валютной пары.
    /// </summary>
    public string Symbol { get; set; } = string.Empty;
    
    /// <summary>
    /// Последняя цена.
    /// </summary>
    public decimal LastPrice { get; set; } 
    
    /// <summary>
    /// Изменение за сутки.
    /// </summary>
    public decimal DailyChange { get; set; } 
    
    /// <summary>
    /// Изменение за сутки в процентах.
    /// </summary>
    public decimal DailyChangePercent { get; set; } 
    
    /// <summary>
    /// Объём торгов.
    /// </summary>
    public decimal Volume { get; set; }
    
    /// <summary>
    /// Максимальная цена за сутки.
    /// </summary>
    public decimal High { get; set; }
    
    
    /// <summary>
    /// Минимальная цена за сутки.
    /// </summary>
    public decimal Low { get; set; } 
}