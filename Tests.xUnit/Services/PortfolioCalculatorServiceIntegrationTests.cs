using BitfinexConnector.Core.Abstractions;
using BitfinexConnector.Core.Models;
using BitfinexConnector.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.xUnit.Services
{
    public class PortfolioCalculatorServiceTests
    {
        private readonly Mock<ITestConnector> _connectorMock = new();
        private readonly ILogger<PortfolioCalculatorService> _logger;

        public PortfolioCalculatorServiceTests()
        {
            _logger = Mock.Of<ILogger<PortfolioCalculatorService>>();
            
            // Базовая настройка тикеров для большинства тестов
            _connectorMock.Setup(c => c.GetTickerAsync("BTCUSDT"))
                .ReturnsAsync(new Ticker { LastPrice = 50000 });
            _connectorMock.Setup(c => c.GetTickerAsync("XRPUSDT"))
                .ReturnsAsync(new Ticker { LastPrice = 1 });
            _connectorMock.Setup(c => c.GetTickerAsync("XMRUSDT"))
                .ReturnsAsync(new Ticker { LastPrice = 200 });
            _connectorMock.Setup(c => c.GetTickerAsync("DASHUSDT"))
                .ReturnsAsync(new Ticker { LastPrice = 100 });
        }

        [Fact]
        public async Task CalculatePortfolioAsync_AllTickersAvailable_ReturnsCorrectBalances()
        {
            // Arrange
            var balances = new Dictionary<string, decimal>
            {
                { "BTC", 1 },
                { "XRP", 1000 }
            };

            var service = new PortfolioCalculatorService(_connectorMock.Object, _logger);

            // Act
            var result = await service.CalculatePortfolioAsync(balances);

            // Assert
            Assert.Equal(51000m, result["USDT"]);
            Assert.Equal(51000m / 50000, result["BTC"]);
            Assert.Equal(51000m / 1, result["XRP"]);
            Assert.Equal(51000m / 200, result["XMR"]);
            Assert.Equal(51000m / 100, result["DASH"]);

            VerifyAllTickersRequested();
        }

        [Fact]
        public async Task CalculatePortfolioAsync_MissingTicker_ThrowsException()
        {
            // Arrange
            _connectorMock.Reset();
            _connectorMock.Setup(c => c.GetTickerAsync("BTCUSDT"))
                .ReturnsAsync((Ticker)null);

            var service = new PortfolioCalculatorService(_connectorMock.Object, _logger);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                service.CalculatePortfolioAsync(new Dictionary<string, decimal>()));
        }

        [Fact]
        public async Task CalculatePortfolioAsync_ZeroBalances_CalculatesCorrectly()
        {
            // Arrange
            var balances = new Dictionary<string, decimal>
            {
                { "BTC", 0 },
                { "XRP", 0 }
            };

            var service = new PortfolioCalculatorService(_connectorMock.Object, _logger);

            // Act
            var result = await service.CalculatePortfolioAsync(balances);

            // Assert
            Assert.All(result.Values, v => Assert.Equal(0m, v));
            VerifyAllTickersRequested();
        }

        [Fact]
        public async Task CalculatePortfolioAsync_ZeroPriceInTicker_SkipsDivisionByZero()
        {
            // Arrange
            _connectorMock.Setup(c => c.GetTickerAsync("BTCUSDT"))
                .ReturnsAsync(new Ticker { LastPrice = 0 });

            var balances = new Dictionary<string, decimal> { { "BTC", 1 } };

            var service = new PortfolioCalculatorService(_connectorMock.Object, _logger);

            // Act
            var result = await service.CalculatePortfolioAsync(balances);

            // Assert
            Assert.Equal(0m, result["BTC"]);
            Assert.Equal(0m, result["USDT"]);
            Assert.Equal(0m, result["XRP"]);
            Assert.Equal(0m, result["XMR"]);
            Assert.Equal(0m, result["DASH"]);

            VerifyAllTickersRequested();
        }

        private void VerifyAllTickersRequested()
        {
            _connectorMock.Verify(c => c.GetTickerAsync("BTCUSDT"), Times.Once);
            _connectorMock.Verify(c => c.GetTickerAsync("XRPUSDT"), Times.Once);
            _connectorMock.Verify(c => c.GetTickerAsync("XMRUSDT"), Times.Once);
            _connectorMock.Verify(c => c.GetTickerAsync("DASHUSDT"), Times.Once);
        }
    }
}