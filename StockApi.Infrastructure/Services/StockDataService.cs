using StockApi.Core.Interfaces;
using StockApi.Core.Models;
using Microsoft.Extensions.Logging;
using StockApi.Core.Inerfaces;

namespace StockApi.Infrastructure.Services;

public class StockDataService : IStockDataService
{
    private readonly IStockDataProvider _provider;
    private readonly ILogger<StockDataService> _logger;

    public StockDataService(IStockDataProvider provider, ILogger<StockDataService> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    public async Task<List<DailyStockData>> GetDailyAggregatesAsync(string symbol, int days = 30, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Aggregating data for {Symbol} over {Days} days", symbol, days);

        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddDays(-days);

        var rawData = await _provider.FetchIntradayDataAsync(symbol, startDate, endDate, cancellationToken);
        return AggregateByDay(rawData);
    }

    private List<DailyStockData> AggregateByDay(RawStockData data)
    {
        var grouped = new Dictionary<string, List<(decimal? low, decimal? high, long? volume)>>();

        for (int i = 0; i < data.Timestamps.Count; i++)
        {
            var date = DateTimeOffset.FromUnixTimeSeconds(data.Timestamps[i]).ToString("yyyy-MM-dd");
            if (!grouped.ContainsKey(date))
            {
                grouped[date] = new List<(decimal?, decimal?, long?)>();
            }
            grouped[date].Add((data.Lows[i], data.Highs[i], data.Volumes[i]));

        }

        _logger.LogInformation("Aggregated {Count} days of data", grouped.Count);
        return grouped.Select(g => new DailyStockData
        {
            Day = g.Key,
            LowAverage = Math.Round(g.Value.Where(v => v.low.HasValue).Average(v => v.low!.Value), 4),
            HighAverage = Math.Round(g.Value.Where(v => v.high.HasValue).Average(v => v.high!.Value), 4),
            Volume = g.Value.Where(v => v.volume.HasValue).Sum(v => v.volume!.Value)
        })
        .OrderBy(d => d.Day)
        .ToList();
    }
}