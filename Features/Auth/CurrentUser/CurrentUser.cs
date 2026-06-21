using System.Security.Claims;

namespace Nexus.Features.Auth.CurrentUser;

public class CurrentUser : ICurrentUser
{
    private IHttpContextAccessor _accessor;
    
    public CurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public Guid UserId
    {
        get
        {
            var claim = _accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (claim == null) throw new UnauthorizedAccessException("UserId is not authenticated");

            return Guid.Parse(claim);
        }
    }

    public string Username
    {
        get
        {
            var claim = _accessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

            if (claim == null) throw new UnauthorizedAccessException("User Name is not authenticated");

            return claim;
        }
    }

    public bool IsAuthenticated => _accessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

}