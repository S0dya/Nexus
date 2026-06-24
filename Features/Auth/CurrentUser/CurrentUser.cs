using System.Security.Claims;

namespace Nexus.Features.Auth.CurrentUser;

public class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid UserId
    {
        get
        {
            var claim = accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (claim == null) throw new UnauthorizedAccessException("UserId is not authenticated");

            return Guid.Parse(claim);
        }
    }

    public string Username
    {
        get
        {
            var claim = accessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

            if (claim == null) throw new UnauthorizedAccessException("User Name is not authenticated");

            return claim;
        }
    }


    public string UserRole
    {
        get
        {
            var claim = accessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;

            if (claim == null) throw new UnauthorizedAccessException("User Role is not authenticated");

            return claim;
        }
    }

    public bool IsAuthenticated => accessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

}