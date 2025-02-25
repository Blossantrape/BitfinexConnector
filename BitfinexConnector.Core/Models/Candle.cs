namespace BitfinexConnector.Core.Models;

/// <summary>
/// Модель свечи для графика цены.
/// </summary>
public class Candle
{
    public DateTime Timestamp { get; set; } // Время свечи.
    public decimal Open { get; set; } // Цена открытия.
    public decimal Close { get; set; } // Цена закрытия.
    public decimal High { get; set; } // Максимальная цена.
    public decimal Low { get; set; } // Минимальная цена.
    public decimal Volume { get; set; } // Объём торгов.
}