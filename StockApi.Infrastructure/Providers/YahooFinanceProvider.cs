using StockApi.Core.Interfaces;
using StockApi.Core.Exceptions;
using StockApi.Infrastructure.Configuration;
using StockApi.Infrastructure.DTOs;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Runtime.CompilerServices;

namespace StockApi.Infrastructure.Providers;

public class YahooFInanceProvider : IStockDataProvider
{
    private readonly HttpClient _httpClient;
    private readonly YahooFinanceOptions _options,
    private readonly ILogger<YahooFInanceProvider> _logger;
    public YahooFInanceProvider(
        HttpClient httpClient,
        MethodImplOptions<YahooFinanceOptions> options,
        ILogger<YahooFInanceProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Add("Uder-Agent", _options.UserAgent);
    }

    public async Task<RawStockData> FetchIntradayDataAsync(
        string symbol,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching data for {Symbol} from {Start} to {End}", symbol, startDate, endDate);

        var startTimestamp = new DateTimeOffset(startDate).ToUnixTimeSeconds();
        var endTimestamp = new DateTimeOffset(endDate).ToUnixTimeSeconds();

        var url = $"{_options.BaseUrl}/{symbol}?interval=15m&period1={startTimestamp}&period2={endTimestamp}";

        try
        {
            var response = await _httpClient.GetStringAsync(url, cancellationToken);
            var data = JsonSerializer.Deserialize<YahooFinanceResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true});

            if (data?.chart?.Error != null)
            {
               _logger.LogWarning("Yahoo Finance API error for {Symbol}: {Error}", symbol, data.Chart.Error.Description);
               throw new InvalidSymbolException(symbol);
            }

            if (data?.chart?.Result == null || !data.chart.Result.Any())
            {
                _logger.LogWarning("No data returned for {Symbol}", symbol);
                throw new InvalidSymbolException(symbol);
            }

            var result = data.chart.Result[0];
            var quote = result.Indicators.Quote[0];

            _logger.LogInformation("Successfully fetched {Count} data points for {Symbol}", result.Timestamp.Count, symbol);

            return new RawStockData
            {
                Timestamps = result.Timestamp,
                Lows = quote.Low,
                Highs = quote.High,
                Volumes = quote.Volume
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching data for {Symbol}", symbol);
            throw new ExternalServiceException($"Failed to fetch data for {symbol} from Yahoo Finance", ex);

        }
        catch (JsonException ex)
        {
            
        }
    }
}