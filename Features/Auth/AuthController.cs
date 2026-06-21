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
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUser _currentUser;
    
    public AuthController(IAuthService authService,
        ICurrentUser currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
    }

    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody]LoginRequest request)
    {
        var response = await _authService.Login(request);
        return Ok(response);
    }
    
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody]RegisterRequest request)
    {
        var response = await _authService.Register(request);
        return Ok(response);
    }

    [EnableRateLimiting(RateLimitPolicies.Reads)]
    [Authorize]
    [HttpGet("get-current-user")]
    public ActionResult<UserResponse> Me()
    {
        return Ok(new UserResponse
        {
            UserId = _currentUser.UserId,
            Username  = _currentUser.Username,
        });
    }
}