namespace Nexus.Features.Auth.CurrentUser;

public interface ICurrentUser
{
    Guid UserId { get; }
    string Username { get; }
    string UserRole { get; }
    bool IsAuthenticated { get; }
}