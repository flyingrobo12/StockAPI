namespace StockApi.Infrastructure.Configuration;

public class YahooFinanceOptions
{
    public const string SectionName = "YahooFinance";

    public string BaseUrl { get; set; } = "https://query1.finance.yahoo.com/v8/finance/chart";

    public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";
    public int TimeoutSeconds { get; set; } = 30;
    
}