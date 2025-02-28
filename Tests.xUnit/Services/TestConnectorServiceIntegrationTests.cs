using System.Reflection;
using BitfinexConnector.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace Tests.xUnit.Services
{
    public class TestConnectorServiceIntegrationTests : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly TestConnectorService _connector;
        private readonly ILogger<TestConnectorService> _logger;

        public TestConnectorServiceIntegrationTests()
        {
            _httpClient = new HttpClient();
            _logger = LoggerFactory.Create(b => b.AddConsole())
                .CreateLogger<TestConnectorService>();
            _connector = new TestConnectorService(_httpClient, _logger);
        }

        public void Dispose() => _httpClient.Dispose();

        [Theory]
        [InlineData("BTCUSDT", "BTCUSD")]
        [InlineData("dashusdt", "DSHUSD")]
        [InlineData(" XRP-USDT ", "XRPUSD")]
        [InlineData("XRP-USD", "XRPUSD")]
        public void ValidateSymbol_CorrectsSymbolFormat(string input, string expected)
        {
            var result = InvokeValidateSymbol(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("BTC")] // Невалидный формат (нет USD)
        [InlineData("USDT")] // Невалидный формат (нет базовой валюты)
        [InlineData("123")] // Невалидный формат (цифры)
        [InlineData("BTC-ETH")] // Невалидный формат (не оканчивается на USD)
        public void ValidateSymbol_InvalidFormat_ThrowsException(string invalidSymbol)
        {
            var ex = Assert.Throws<ArgumentException>(() => InvokeValidateSymbol(invalidSymbol));
            Assert.Contains("Неверный формат символа", ex.Message);
        }

        private string InvokeValidateSymbol(string symbol)
        {
            var method = _connector.GetType()
                .GetMethod("ValidateSymbol",
                    BindingFlags.NonPublic |
                    BindingFlags.Instance);

            try
            {
                return (string)method.Invoke(_connector, new object[] { symbol });
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        [Theory]
        [InlineData("BTCUSD")]
        [InlineData("ETHUSD")]
        [InlineData("XRPUSD")]
        public async Task GetTickerAsync_ValidSymbol_ReturnsValidTicker(string symbol)
        {
            var result = await _connector.GetTickerAsync(symbol);
            Assert.NotNull(result);
            Assert.True(result.LastPrice > 0);
            Assert.Equal(symbol, result.Symbol);
        }

        [Fact]
        public async Task GetTickerAsync_InvalidSymbol_ReturnsNull()
        {
            var result = await _connector.GetTickerAsync("INVALIDPAIR");
            Assert.Null(result);
        }

        [Theory]
        [InlineData("BTCUSD", 10)]
        [InlineData("ETHUSD", 5)]
        public async Task GetTradesAsync_ValidSymbol_ReturnsTrades(string symbol, int limit)
        {
            var result = await _connector.GetTradesAsync(symbol, limit);
            Assert.NotNull(result);
            Assert.All(result, trade =>
            {
                Assert.True(trade.Price > 0);
                Assert.NotEqual(0, trade.Amount);
            });
        }

        [Fact]
        public async Task GetTradesAsync_InvalidSymbol_ReturnsEmptyList()
        {
            // Act
            var result = await _connector.GetTradesAsync("INVALIDPAIR", 10);

            // Assert
            Assert.Empty(result);
        }

        [Theory]
        [InlineData("BTCUSD", "1m", 10)]
        [InlineData("ETHUSD", "1h", 5)]
        public async Task GetCandlesAsync_ValidParameters_ReturnsCandles(string symbol, string timeFrame, int limit)
        {
            var result = await _connector.GetCandlesAsync(symbol, timeFrame, limit);
            Assert.NotNull(result);
            Assert.All(result, candle =>
            {
                Assert.True(candle.High >= candle.Low);
                Assert.True(candle.Volume >= 0);
            });
        }
    }
}