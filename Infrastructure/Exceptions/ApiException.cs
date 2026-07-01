namespace Nexus.Infrastructure.Exceptions;

public abstract class ApiException : Exception
{
    public int StatusCode { get; }

    protected ApiException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    protected ApiException(string message, int statusCode, Exception innerException) : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}
