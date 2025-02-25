namespace BitfinexConnector.Core.Models;

/// <summary>
/// Модель свечи для графика цены.
/// </summary>
public class Candle
{
    /// <summary>
    /// Время свечи.
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Цена открытия.
    /// </summary>
    public decimal Open { get; set; }
    
    /// <summary>
    /// Цена закрытия.
    /// </summary>
    public decimal Close { get; set; }
    
    /// <summary>
    /// Максимальная цена.
    /// </summary>
    public decimal High { get; set; }
    
    /// <summary>
    /// Минимальная цена.
    /// </summary>
    public decimal Low { get; set; }
    
    /// <summary>
    /// Объём торгов.
    /// </summary>
    public decimal Volume { get; set; }
}