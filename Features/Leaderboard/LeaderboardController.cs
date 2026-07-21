using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Nexus.Features.Leaderboard.Dto;
using Nexus.Features.Leaderboard.Services;
using Nexus.Infrastructure.DependencyInjection.RateLimiting;

namespace Nexus.Features.Leaderboard;

[ApiController]
[Route("leaderboard")]
public class LeaderboardController(ILeaderboardService leaderboardService) : ControllerBase
{
    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [HttpPost("submit")]
    [Authorize]
    public async Task<ActionResult<SubmitScoreResponse>> SubmitScore([FromBody] SubmitScoreRequest request)
    {
        var response = await leaderboardService.SubmitScore(request);
        return Ok(response);
    }

    [EnableRateLimiting(RateLimitPolicies.Reads)]
    [HttpGet("global")]
    [Authorize]
    public async Task<ActionResult<GlobalLeaderboardResponse>> GetGlobalLeaderboard(GlobalLeaderboardRequest request)
    {
        var response = await leaderboardService.GetGlobalLeaderboard(request);
        return Ok(response);
    }

    [EnableRateLimiting(RateLimitPolicies.Reads)]
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<MyLeaderboardResponse>> GetMyLeaderboard()
    {
        var response = await leaderboardService.GetMyLeaderboard();
        return Ok(response);
    }

    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [HttpPost("season/reset")]
    [Authorize]
    public async Task<ActionResult> ResetSeason()
    {
        // await leaderboardService.ResetSeason();
        return Ok();
    }
}
