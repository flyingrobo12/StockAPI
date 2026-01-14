using StockApi.Api.Middleware;
using StockApi.Core.Interfaces;
using StockApi.Infrastructure.Configuration;
using StockApi.Infrastructure.Providers;
using StockApi.Infrastructure.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Services
builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Stock API",
        Version = "v1",
        Description = "API for retrieving aggregated stock market data"
    });
});

// Configuration
builder.Services.Configure<YahooFinanceOptions>(builder.Configuration.GetSection(YahooFinanceOptions.SectionName));

builder.Services.AddHttpClient<IStockDataProvider, YahooFinanceProvider>();
builder.Services.AddScoped<IStockDataService, StockDataService>();

var app = builder.Build();

// Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging();
app.MapControllers();

try
{
    Log.Information("Starting Stock API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}