namespace Nexus.Infrastructure.Exceptions;

public class ConflictException : ApiException
{
    public ConflictException(string message) : base(message, 409)
    {
    }

    public ConflictException(string message, Exception innerException) : base(message, 409, innerException)
    {
    }
}
