using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Nexus.Features.Auth.CurrentUser;
using Nexus.Features.Profile.Dto;
using Nexus.Features.Profile.Services;
using Nexus.Infrastructure.DependencyInjection.RateLimiting;

namespace Nexus.Features.Profile;

[ApiController]
[Route("profile")]
public class ProfileController(
    IProfileService profileService) : ControllerBase
{
    [EnableRateLimiting(RateLimitPolicies.Reads)]
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ProfileResponse>> GetUser(Guid userId)
    {
        var response = await profileService.GetUser(userId);
        return Ok(response);
    }

    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [HttpPatch("me")]
    [Authorize]
    public async Task<ActionResult> PatchMe([FromBody] PatchProfileRequest request)
    {
        await profileService.PatchProfile(request);
        return Ok();
    }

    [EnableRateLimiting(RateLimitPolicies.Reads)]
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<FullProfileResponse>> GetMe()
    {
        var response = await profileService.GetMe();
        return Ok(response);
    }
}
