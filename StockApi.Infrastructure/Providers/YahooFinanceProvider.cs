using StockApi.Core.Interfaces;
using StockApi.Core.Exceptions;
using StockApi.Infrastructure.Configuration;
using StockApi.Infrastructure.DTOs;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace StockApi.Infrastructure.Providers;

public class YahooFinanceProvider : IStockDataProvider
{
    private readonly HttpClient _httpClient;
    private readonly YahooFinanceOptions _options;
    private readonly ILogger<YahooFinanceProvider> _logger;

    public YahooFinanceProvider(
        HttpClient httpClient,
        IOptions<YahooFinanceOptions> options,
        ILogger<YahooFinanceProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", _options.UserAgent);
    }

    public async Task<RawStockData> FetchIntradayDataAsync(
        string symbol,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching data for {Symbol} from {Start} to {End}", 
            symbol, startDate, endDate);

        var url = $"{_options.BaseUrl}/{symbol}?interval=15m&range=1mo";

        try
        {
            var response = await _httpClient.GetStringAsync(url, cancellationToken);
            var data = JsonSerializer.Deserialize<YahooFinanceResponse>(response,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (data?.Chart?.Error != null)
            {
                _logger.LogWarning("Yahoo Finance API error for {Symbol}: {Error}",
                    symbol, data.Chart.Error.Description);
                throw new InvalidSymbolException(symbol);
            }

            if (data?.Chart?.Result == null || !data.Chart.Result.Any())
            {
                _logger.LogWarning("No data returned for {Symbol}", symbol);
                throw new InvalidSymbolException(symbol);
            }

            var result = data.Chart.Result[0];
            var quote = result.Indicators.Quote[0];

            _logger.LogInformation("Successfully fetched {Count} data points for {Symbol}",
                result.Timestamp.Count, symbol);

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
            throw new ExternalServiceException(
                $"Failed to fetch data for {symbol} from Yahoo Finance", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error for {Symbol}", symbol);
            throw new ExternalServiceException(
                $"Failed to parse Yahoo Finance response for {symbol}", ex);
        }
    }
}