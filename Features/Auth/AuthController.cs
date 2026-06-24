using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Nexus.Features.Auth.CurrentUser;
using Nexus.Features.Auth.Dto;
using Nexus.Features.Auth.Services;
using Nexus.Infrastructure.DependencyInjection.RateLimiting;

namespace Nexus.Features.Auth;

[ApiController]
[Route("auth")]
public class AuthController(
    IDbAuthService dbAuthService,
    ICurrentUser currentUser) : ControllerBase
{
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [HttpPost("anonymous")]
    public async Task<ActionResult<AuthResponse>> Anonymous([FromBody]AnonymousRequest request)
    {
        var response = await dbAuthService.Anonymous(request);
        return Ok(response);
    }
    
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody]LoginRequest request)
    {
        var response = await dbAuthService.Login(request);
        return Ok(response);
    }
    
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody]RegisterRequest request)
    {
        var response = await dbAuthService.Register(request);
        return Ok(response);
    }
    
    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [HttpPost("refresh")]
    [Authorize]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody]RefreshRequest request)
    {
        var response = await dbAuthService.Refresh(request);
        return Ok(response);
    }
    
    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<AuthResponse>> Logout([FromBody]LogoutRequest request)
    {
        await dbAuthService.Logout(request);
        return Ok();
    }

    [EnableRateLimiting(RateLimitPolicies.Reads)]
    [Authorize]
    [HttpGet("get-current-user")]
    public ActionResult<UserResponse> Me()
    {
        return Ok(new UserResponse
        {
            UserId = currentUser.UserId,
            Username  = currentUser.Username,
        });
    }
}