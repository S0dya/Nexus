namespace Nexus.Infrastructure.Exceptions;

public class ValidationException : ApiException
{
    public ValidationException(string message) : base(message, 400)
    {
    }

    public ValidationException(string message, Exception innerException) : base(message, 400, innerException)
    {
    }
}
