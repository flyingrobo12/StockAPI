using StockApi.Core.Models;

namespace StockApi.Core.Interfaces;

public interface IStockDataService
{
    Task<List<DailyStockData>> GetDailyAggregatesAsync(
        string symbol,
        int days = 30,
        CancellationToken cancellationToken = default);
}