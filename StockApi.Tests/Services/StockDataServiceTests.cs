using StockApi.Core.Interfaces;
using StockApi.Core.Models;
using StockApi.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;

namespace StockApi.Tests.Services;

public class StockDataServiceTests
{
    private readonly Mock<IStockDataProvider> _mockProvider;
    private readonly Mock<ILogger<StockDataService>> _mockLogger;
    private readonly StockDataService _service;

    public StockDataServiceTests()
    {
        _mockProvider = new Mock<IStockDataProvider>();
        _mockLogger = new Mock<ILogger<StockDataService>>();
        _service = new StockDataService(_mockProvider.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetDailyAggregates_WithValidData_ReturnsAggregatedResults()
    {
        //arrange the data
        var rawData = new RawStockData
        {
            Timestamps = new List<long>
            {
                1768003200, // 1/10/26 0 UTC
                1768006800, // 1/10/26 1 UTC
                1768089600 //1/11/26 0 UTC
            },
            Lows = new List<decimal?> { 100m, 101m, 102m },
            Highs = new List<decimal?> { 110m, 111m, 112m },
            Volumes = new List<long?> {1000, 2000, 3000}
        };

        _mockProvider.Setup(p => p.FetchIntradayDataAsync(
            It.IsAny<string>(),
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(rawData);
        
        var result = await _service.GetDailyAggregatesAsync("GOOGL", 30);

        var day1 = result.First(d => d.Day == "2026-01-10");
        day1.LowAverage.Should().Be(100.5m); // (100+101)/2
        day1.HighAverage.Should().Be(110.5m);
        day1.Volume.Should().Be(3000); // 1k+2k

        var day2 = result.First(d => d.Day == "2026-01-11");
        day2.LowAverage.Should().Be(102m);
        day2.HighAverage.Should().Be(112m);
        day2.Volume.Should().Be(3000);
    }
    [Fact]
    public async Task GetDailyAggregates_WithNullValues_IgnoreNulls()
    {
        var rawData = new RawStockData
        {
            Timestamps = new List<long>
            {
                1767571200, 1767574800, 1767578400 //Jan 5th
            },
            Lows = new List<decimal?> {100m, null, 102m},
            Highs = new List<decimal?> {null, 112m, 115m},
            Volumes = new List<long?> { 1000, null, 2000}
        };
        _mockProvider.Setup(p => p.FetchIntradayDataAsync(
            It.IsAny<string>(),
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(rawData);
        
        var result = await _service.GetDailyAggregatesAsync("RIVN", 30);
        result.Should().HaveCount(1);
        result[0].LowAverage.Should().Be(101m); 
        result[0].HighAverage.Should().Be(113.5m); // 112+115/2
        result[0].Volume.Should().Be(3000);
    }

    [Fact]
    public async Task GetDailyAggregates_WithPrecision_RoundsToFourDecimals()
    {
        var rawData = new RawStockData
        {
            Timestamps = new List<long>
            {
                1767657600, 1767661200 // Jan 6th
            },
            Lows = new List<decimal?> {100.123456m, 100.987654m},
            Highs = new List<decimal?> {200.111111m, 200.999999m},
            Volumes = new List<long?> {1000, 2000}
        };
        _mockProvider.Setup(p => p.FetchIntradayDataAsync(
            It.IsAny<string>(),
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(rawData);
        
        var result = await _service.GetDailyAggregatesAsync("META", 30);
        result[0].LowAverage.Should().Be(100.5556m);
        result[0].HighAverage.Should().Be(200.5556m);
        
    }

    [Fact]
    public async Task GetDailyAggregates_ResultsAreSortedByDate()
    {
        var rawData = new RawStockData
        {
            Timestamps = new List<long>
            {
                1767744000, //Jan 7th
                1767571200, // Jan 5th
                1767657600 // Jan 6th
            },
            Lows = new List<decimal?> {100m, 101m, 102m},
            Highs = new List<decimal?> {110m, 111m, 112m},
            Volumes = new List<long?> {1000, 2000,3000},
        };

        _mockProvider.Setup(p => p.FetchIntradayDataAsync(
            It.IsAny<string>(),
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(rawData);
        
        var result = await _service.GetDailyAggregatesAsync("NVDA", 30);
        result.Should().HaveCount(3);
        result[0].Day.Should().Be("2026-01-05");
        result[1].Day.Should().Be("2026-01-06");
        result[2].Day.Should().Be("2026-01-07");
    }
}