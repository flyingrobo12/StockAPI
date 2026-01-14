namespace StockApi.Infrastructure.DTOs;

internal class YahooFinanceResponse
{
    public Chart Chart { get; set; } = null!;
}

internal class Chart
{
    public List<Result> Result { get; set; } = null!;
    public Error Error { get; set; } = null!;
}

internal class Result
{
    public Meta Meta { get; set; } = null!;
    public List<long> Timestamp { get; set; } = null!;
    public Indicators Indicators { get; set; } = null!;
}

internal class Meta
{
    public string Symbol { get; set; } = null!;
    public string Currency { get; set; } = null!;
}

internal class Indicators
{
    public List<Quote> Quote { get; set; } = null!;
}

internal class Quote
{
    public List<decimal?> Low { get; set; } = null!;
    public List<decimal?> High { get; set; } = null!;
    public List<long?> Volume { get; set; } = null!;
}

internal class Error
{
    public string Code { get; set; } = null!;
    public string Description { get; set; } = null!;
}