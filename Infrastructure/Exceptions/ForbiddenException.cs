namespace Nexus.Infrastructure.Exceptions;

public class ForbiddenException : ApiException
{
    public ForbiddenException(string message) : base(message, 403)
    {
    }

    public ForbiddenException(string message, Exception innerException) : base(message, 403, innerException)
    {
    }
}
