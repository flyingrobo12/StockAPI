using StockApi.Core.Exceptions;
using StockApi.Infrastructure.Configuration;
using StockApi.Infrastructure.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using FluentAssertions;

namespace StockApi.Tests.Providers;

public class YahooFinanceProviderTests
{
    private readonly Mock<ILogger<YahooFinanceProvider>> _mockLogger;
    private readonly IOptions<YahooFinanceOptions> _options;

    public YahooFinanceProviderTests()
    {
        _mockLogger = new Mock<ILogger<YahooFinanceProvider>>();
        _options = Options.Create(new YahooFinanceOptions
        {
            BaseUrl = "https://query1.finance.yahoo.com/v8/finance/chart",
            UserAgent = "Mozilla/5.0",
            TimeoutSeconds = 30
        });
    }

    [Fact]
    public void Constructor_ConfiguresTimeout()
    {
        var httpClient = new HttpClient();
        var provider = new YahooFinanceProvider(httpClient, _options, _mockLogger.Object);
        
        httpClient.Timeout.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void Constructor_UsesCustomOptionsCorrectly()
    {
        var customOptions = Options.Create(new YahooFinanceOptions
        {
            BaseUrl = "https://custom-url.com",
            UserAgent = "CustomAgent",
            TimeoutSeconds = 60
        });
        
        var httpClient = new HttpClient();
        var provider = new YahooFinanceProvider(httpClient, customOptions, _mockLogger.Object);
        
        httpClient.Timeout.Should().Be(TimeSpan.FromSeconds(60));
    }
}