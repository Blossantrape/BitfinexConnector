using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Tests.xUnit
{
    /// <summary>
    /// Интеграционные тесты для контроллера PortfolioController.
    /// Тестируется эндпоинт получения баланса портфеля.
    /// </summary>
    public class PortfolioControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public PortfolioControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetPortfolioBalance_ReturnsOk()
        {
            // Act
            var response = await _client.GetAsync("/api/portfolio");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
