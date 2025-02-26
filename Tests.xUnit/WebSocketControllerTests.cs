using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Tests.xUnit
{
    /// <summary>
    /// Интеграционные тесты для WebSocketController.
    /// Тестируются эндпоинты подписки на тикер, трейды и свечи.
    /// </summary>
    public class WebSocketControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public WebSocketControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task SubscribeTicker_ReturnsOk()
        {
            // Arrange
            string symbol = "BTCUSD";
            var request = new HttpRequestMessage(HttpMethod.Post, $"/api/websocket/subscribe/ticker/{symbol}")
            {
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SubscribeTrades_ReturnsOk()
        {
            // Arrange
            string symbol = "BTCUSD";
            var request = new HttpRequestMessage(HttpMethod.Post, $"/api/websocket/subscribe/trades/{symbol}")
            {
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SubscribeCandles_ReturnsOk()
        {
            // Arrange
            string symbol = "BTCUSD";
            string timeframe = "1m";
            var request =
                new HttpRequestMessage(HttpMethod.Post, $"/api/websocket/subscribe/candles/{symbol}/{timeframe}")
                {
                    Content = new StringContent("", Encoding.UTF8, "application/json")
                };

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}