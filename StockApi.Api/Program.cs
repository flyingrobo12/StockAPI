using StockApi.Api.Middleware;
using StockApi.Core.Interfaces;
using StockApi.Infrastructure.Configuration;
using StockApi.Infrastructure.Providers;
using StockApi.Infrastructure.Services;
using Serilog;
using StockApi.Core.Inerfaces;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).WriteTo.Console().CreateLogger();

builder.Host.UseSerilog();
//adding in services
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

//config yahoo options
builder.Services.Configure<YahooFinanceOptions>(builder.Configuration.GetSection(YahooFinanceOptions.SectionName));
//register services with DI
builder.Services.AddHttpClient<IStockDataProvider, YahooFinanceProvider>();
builder.Services.AddScoped<IStockDataService, StockDataService>();

var app = builder.Build();

//middleware pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
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