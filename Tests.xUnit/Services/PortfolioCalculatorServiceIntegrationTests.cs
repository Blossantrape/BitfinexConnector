using BitfinexConnector.Core.Abstractions;
using BitfinexConnector.Core.Models;
using BitfinexConnector.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.xUnit.Services
{
    public class PortfolioCalculatorServiceTests
    {
        private ILogger<PortfolioCalculatorService> CreateLogger() =>
            Mock.Of<ILogger<PortfolioCalculatorService>>();

        private Mock<ITestConnector> CreateConnectorMock()
        {
            var connectorMock = new Mock<ITestConnector>();
            // Базовая настройка тикеров для большинства тестов
            connectorMock.Setup(c => c.GetTickerAsync("BTCUSDT"))
                .ReturnsAsync(new Ticker { LastPrice = 50000 });
            connectorMock.Setup(c => c.GetTickerAsync("XRPUSDT"))
                .ReturnsAsync(new Ticker { LastPrice = 1 });
            connectorMock.Setup(c => c.GetTickerAsync("XMRUSDT"))
                .ReturnsAsync(new Ticker { LastPrice = 200 });
            connectorMock.Setup(c => c.GetTickerAsync("DASHUSDT"))
                .ReturnsAsync(new Ticker { LastPrice = 100 });
            return connectorMock;
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

            var connectorMock = CreateConnectorMock();
            var service = new PortfolioCalculatorService(connectorMock.Object, CreateLogger());

            // Act
            var result = await service.CalculatePortfolioAsync(balances);

            // Assert
            Assert.Equal(51000m, result["USDT"]);
            Assert.Equal(51000m / 50000, result["BTC"]);
            Assert.Equal(51000m / 1, result["XRP"]);
            Assert.Equal(51000m / 200, result["XMR"]);
            Assert.Equal(51000m / 100, result["DASH"]);

            VerifyAllTickersRequested(connectorMock);
        }

        [Fact]
        public async Task CalculatePortfolioAsync_MissingTicker_ThrowsException()
        {
            // Arrange
            var connectorMock = CreateConnectorMock();
            // Настраиваем отсутствие тикера для BTCUSDT
            connectorMock.Setup(c => c.GetTickerAsync("BTCUSDT"))
                .ReturnsAsync((Ticker)null);

            var service = new PortfolioCalculatorService(connectorMock.Object, CreateLogger());

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

            var connectorMock = CreateConnectorMock();
            var service = new PortfolioCalculatorService(connectorMock.Object, CreateLogger());

            // Act
            var result = await service.CalculatePortfolioAsync(balances);

            // Assert
            Assert.All(result.Values, v => Assert.Equal(0m, v));
            VerifyAllTickersRequested(connectorMock);
        }

        [Fact]
        public async Task CalculatePortfolioAsync_ZeroPriceInTicker_SkipsDivisionByZero()
        {
            // Arrange
            var connectorMock = CreateConnectorMock();
            connectorMock.Setup(c => c.GetTickerAsync("BTCUSDT"))
                .ReturnsAsync(new Ticker { LastPrice = 0 });

            var balances = new Dictionary<string, decimal> { { "BTC", 1 } };
            var service = new PortfolioCalculatorService(connectorMock.Object, CreateLogger());

            // Act
            var result = await service.CalculatePortfolioAsync(balances);

            // Assert
            Assert.Equal(0m, result["BTC"]);
            Assert.Equal(0m, result["USDT"]);
            Assert.Equal(0m, result["XRP"]);
            Assert.Equal(0m, result["XMR"]);
            Assert.Equal(0m, result["DASH"]);

            VerifyAllTickersRequested(connectorMock);
        }

        private void VerifyAllTickersRequested(Mock<ITestConnector> connectorMock)
        {
            connectorMock.Verify(c => c.GetTickerAsync("BTCUSDT"), Times.Once);
            connectorMock.Verify(c => c.GetTickerAsync("XRPUSDT"), Times.Once);
            connectorMock.Verify(c => c.GetTickerAsync("XMRUSDT"), Times.Once);
            connectorMock.Verify(c => c.GetTickerAsync("DASHUSDT"), Times.Once);
        }
    }
}