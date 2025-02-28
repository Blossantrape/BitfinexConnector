using BitfinexConnector.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace BitfinexConnector.Controllers
{
    /// <summary>
    /// Контроллер для расчёта и получения баланса портфеля.
    /// Балансы зафиксированы: 1 BTC, 15000 XRP, 50 XMR, 30 DASH.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioCalculatorController : ControllerBase
    {
        private readonly PortfolioCalculatorService _calculatorService;

        public PortfolioCalculatorController(PortfolioCalculatorService calculatorService)
        {
            _calculatorService = calculatorService;
        }

        /// <summary>
        /// Получает общий баланс портфеля в USDT, BTC, XRP, XMR и DASH.
        /// Пример запроса: GET /api/portfolio
        /// </summary>
        /// <returns>Словарь, где ключ – валюта, а значение – баланс.</returns>
        [HttpGet]
        public async Task<IActionResult> GetPortfolioBalance()
        {
            // Фиксированные балансы для расчёта
            var balances = new Dictionary<string, decimal>
            {
                { "BTC", 1m },
                { "XRP", 15000m },
                { "XMR", 50m },
                { "DASH", 30m }
            };

            var result = await _calculatorService.CalculatePortfolioAsync(balances);
            return Ok(result);
        }
    }
}