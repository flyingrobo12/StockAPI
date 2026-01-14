using Microsoft.AspNetCore.Mvc;
using StockApi.Core.Interfaces;
using StockApi.Core.Models;
using Microsoft.Extensions.Caching.Memory;

namespace StockApi.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class StockController : ControllerBase
{
    private readonly IStockDataService _stockDataService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<StockController> _logger;

    public StockController(
        IStockDataService stockDataService,
        IMemoryCache cache,
        ILogger<StockController> logger)
    {
        _stockDataService = stockDataService;
        _cache = cache;
        _logger = logger;
    }

    [HttpGet("{symbol}")]
    public async Task<ActionResult<List<DailyStockData>>> GetStockData(
        string symbol,
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            return BadRequest(new { error = "Symbol is required" });

        if (days < 1 || days > 365)
            return BadRequest(new { error = "Days must be between 1 and 365" });

        symbol = symbol.ToUpperInvariant();
        var cacheKey = $"stock_{symbol}_{days}";

        if (_cache.TryGetValue<List<DailyStockData>>(cacheKey, out var cachedData))
        {
            _logger.LogInformation("Returning cached data for {Symbol}", symbol);
            return Ok(cachedData);
        }

        var data = await _stockDataService.GetDailyAggregatesAsync(
            symbol, days, cancellationToken);

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(15));
        
        _cache.Set(cacheKey, data, cacheOptions);

        _logger.LogInformation("Successfully retrieved {Count} days for {Symbol}", 
            data.Count, symbol);

        return Ok(data);
    }
}