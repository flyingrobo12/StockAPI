namespace StockApi.Core.Models;

public class DailyStockData
{
    public string Day { get; set; } = null!;
    public decimal LowAverage { get; set; }
    public decimal HighAverage { get; set; }
    public long Volume { get; set; }
}