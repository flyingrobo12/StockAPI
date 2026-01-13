using StockApi.Core.Models;

namespace StockApi.Core.Inerfaces;

public interface IStockDataService
{
    Task<List<DailyStockData>> GetDailyAggregatesAsync(
        string symbol,
        int days = 30,
        CancellationToken cancellationToken = default);
}