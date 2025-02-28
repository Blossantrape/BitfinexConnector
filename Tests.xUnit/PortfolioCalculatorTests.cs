using BitfinexConnector.Core.Abstractions;
using BitfinexConnector.Core.Models;
using BitfinexConnector.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.xUnit
{
    public class PortfolioCalculatorTests
    {
        private readonly Mock<ITestConnector> _connectorMock;
        private readonly Mock<ILogger<PortfolioCalculator>> _loggerMock;
        private readonly PortfolioCalculator _calculator;

        public PortfolioCalculatorTests()
        {
            _connectorMock = new Mock<ITestConnector>();
            _loggerMock = new Mock<ILogger<PortfolioCalculator>>();
            _calculator = new PortfolioCalculator(_connectorMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task CalculatePortfolioAsync_ShouldReturnCorrectValues()
        {
            // Arrange
            _connectorMock.Setup(x => x.GetTickerAsync("BTCUSDT")).ReturnsAsync(new Ticker { LastPrice = 50000 });
            _connectorMock.Setup(x => x.GetTickerAsync("XRPUSDT")).ReturnsAsync(new Ticker { LastPrice = 1 });
            _connectorMock.Setup(x => x.GetTickerAsync("XMRUSDT")).ReturnsAsync(new Ticker { LastPrice = 200 });
            _connectorMock.Setup(x => x.GetTickerAsync("DASHUSDT")).ReturnsAsync(new Ticker { LastPrice = 150 });

            var balances = new Dictionary<string, decimal>
            {
                { "BTC", 1 },
                { "XRP", 15000 },
                { "XMR", 50 },
                { "DASH", 30 }
            };

            // Act
            var result = await _calculator.CalculatePortfolioAsync(balances);

            // Assert
            Assert.Equal(50000 + 15000 * 1 + 50 * 200 + 30 * 150, result["USDT"]);
        }

        [Fact]
        public async Task CalculatePortfolioAsync_ShouldThrowException_WhenTickerIsNull()
        {
            // Arrange
            _connectorMock.Setup(x => x.GetTickerAsync("BTCUSDT")).ReturnsAsync((Ticker)null);

            var balances = new Dictionary<string, decimal>
            {
                { "BTC", 1 }
            };

            // Act & Assert
            await Assert.ThrowsAsync<System.Exception>(() => _calculator.CalculatePortfolioAsync(balances));
        }
    }
}
