using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Nexus.Infrastructure.DependencyInjection.RateLimiting;

namespace Nexus.Features.Health;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    [EnableRateLimiting(RateLimitPolicies.Reads)]
    [HttpGet]
    public ActionResult<string> Check()
    {
        return Ok("Ok");
    }
}
