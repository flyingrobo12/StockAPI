namespace StockApi.Core.Models;

public class StockDataRequest
{
    public string Symbol { get; set; } = null!;
    public int Days { get; set; } = 30;
}