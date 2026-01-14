# Stock API

A .NET 8 API that pulls stock data from Yahoo Finance and aggregates it by day. Built with Clean Architecture to keep things organized and testable.

## What It Does

Takes a stock symbol, grabs the last month of 15-minute interval data from Yahoo Finance, and rolls it up into daily averages. Returns clean JSON with the low/high averages and total volume for each day.

## Stack

- .NET 8 / C# 12
- Yahoo Finance API (the free one)
- xUnit, Moq, FluentAssertions for testing
- Serilog for logging
- Swagger for API docs

## Running It

Need .NET 8 SDK installed.
```bash
dotnet restore
dotnet run --project StockApi.Api
```

Hit `http://localhost:5168` or check out Swagger at `http://localhost:5168/swagger`

## Testing
```bash
dotnet test
```

Got 6 tests covering the aggregation logic, null handling, precision rounding, and date sorting. All passing.

## Using the API

**Get stock data:**
```
GET /api/v1/stock/AAPL?days=30
```

Returns something like:
```json
[
  {
    "day": "2026-01-10",
    "lowAverage": 258.3950,
    "highAverage": 261.8100,
    "volume": 52108534
  }
]
```

- `symbol` is required (AAPL, TSLA, whatever)
- `days` is optional, defaults to 30, max is 365
- Returns 404 if the symbol doesn't exist
- Returns 502 if Yahoo Finance is down

## How It's Built

Split into three layers to keep dependencies clean:

- **StockApi.Api** - Controllers and middleware
- **StockApi.Core** - Domain models and interfaces (no dependencies)
- **StockApi.Infrastructure** - Yahoo Finance integration
- **StockApi.Tests** - Unit tests with mocked dependencies

Core doesn't know about infrastructure. Infrastructure implements core interfaces. API ties it together with dependency injection.

## Notes

- Caches responses for 15 minutes to avoid hammering Yahoo
- User-Agent header is required or Yahoo blocks the requests
- Yahoo's API can be flaky - added proper error handling for timeouts and rate limits
- All prices rounded to 4 decimals per the spec

## Config

Edit `StockApi.Api/appsettings.json` if you need to change the Yahoo Finance URL or timeout:
```json
{
  "YahooFinance": {
    "BaseUrl": "https://query1.finance.yahoo.com/v8/finance/chart",
    "UserAgent": "Mozilla/5.0",
    "TimeoutSeconds": 30
  }
}
```
