using StockApi.Core.Models;

namespace StockApi.Core.Interfaces;

public interface IStockDataProvider
{
    Task<RawStockData> FetchIntradayDataAsync(
        string symbol,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);
}

public class RawStockData
{
    public List<long> Timestamps { get; set; } = null!;
    public List<decimal?> Lows { get; set; } = null!;
    public List<decimal?> Highs { get; set; } = null!;
    public List<long?> Volumes { get; set; } = null!;
}