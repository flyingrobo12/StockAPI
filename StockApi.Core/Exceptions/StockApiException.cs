namespace StockApi.Core.Exceptions;

public class StockApiException : Exception
{
    public StockApiException(string message) : base(message) {}

    public StockApiException(string message, Exception innerException) : base(message, innerException) {}

}

public class InvalidSymbolException : StockApiException
{
    public InvalidSymbolException(string symbol) : base($"Invalid or unknown stock symbol: {symbol}") {}

}

public class ExternalServiceException : StockApiException
{
    public ExternalServiceException(string message, Exception innerException) : base(message, innerException) {}
    
}

