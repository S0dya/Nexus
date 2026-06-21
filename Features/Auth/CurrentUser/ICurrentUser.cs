namespace Nexus.Features.Auth.CurrentUser;

public interface ICurrentUser
{
    Guid UserId { get; }
    string Username { get; }
    bool IsAuthenticated { get; }
}