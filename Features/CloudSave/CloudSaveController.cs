using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Nexus.Features.CloudSave.Dto;
using Nexus.Features.CloudSave.Services;
using Nexus.Infrastructure.DependencyInjection.RateLimiting;

namespace Nexus.Features.CloudSave;

[ApiController]
[Route("cloudsave")]
public class CloudSaveController(ICloudSaveService cloudSaveService) : ControllerBase
{
    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [HttpPut("me")]
    [Authorize]
    public async Task<ActionResult<SaveDataResponse>> SaveData([FromBody] SaveDataRequest request)
    {
        var response = await cloudSaveService.SaveData(request);
        return Ok(response);
    }

    [EnableRateLimiting(RateLimitPolicies.Reads)]
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<LoadDataResponse>> LoadData()
    {
        var response = await cloudSaveService.LoadData();
        return Ok(response);
    }

    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [HttpPost("me")]
    [Authorize]
    public async Task<ActionResult> ResetSave()
    {
        await cloudSaveService.ResetSave();
        return Ok();
    }
}
