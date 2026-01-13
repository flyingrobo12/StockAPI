using StockApi.Core.Exceptions;
using System.Net;
using System.Text.Json;

namespace StockApi.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception occured");

        var (statusCode, message) = exception switch
        {
            InvalidSymbolException => (HttpStatusCode.NotFound, exception.Message),
            ExternalServiceException => (HttpStatusCode.BadGateway, exception.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occured")

        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            error = message,
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        
    }
}