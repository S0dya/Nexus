using System.Security.Claims;
using Nexus.Features.Profile.Services;

namespace Nexus.Middlewares;

public class LastOnlineMiddleware(
    RequestDelegate next,
    ILogger<LastOnlineMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, IUserActivityService userActivityService)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            await next(context);
            return;
        }

        await next(context);

        try
        {
            await userActivityService.UpdateLastOnline(userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update last online");
        }
    }
}