using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace Tests.xUnit
{
    /// <summary>
    /// Интеграционные тесты для контроллера MarketController.
    /// Тестируются эндпоинты получения трейдов, свечей и тикера.
    /// </summary>
    public class MarketControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public MarketControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetTrades_ReturnsOk()
        {
            // Arrange
            string symbol = "BTCUSD";

            // Act
            var response = await _client.GetAsync($"/api/market/trades/{symbol}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetCandles_ReturnsOk()
        {
            // Arrange
            string symbol = "BTCUSD";
            string timeframe = "1m";

            // Act
            var response = await _client.GetAsync($"/api/market/candles/{symbol}/{timeframe}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetTicker_ReturnsOk()
        {
            // Arrange
            string symbol = "BTCUSD";

            // Act
            var response = await _client.GetAsync($"/api/market/ticker/{symbol}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}