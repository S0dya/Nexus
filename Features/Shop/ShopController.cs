using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Nexus.Features.Shop.Dto;
using Nexus.Features.Shop.Services;
using Nexus.Infrastructure.DependencyInjection.RateLimiting;

namespace Nexus.Features.Shop;

[ApiController]
[Route("shop")]
public class ShopController(IShopService shopService) : ControllerBase
{
    [EnableRateLimiting(RateLimitPolicies.Reads)]
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<GetShopOffersResponse>> GetAllOffers()
    {
        var response = await shopService.GetAllOffers();
        return Ok(response);
    }

    [EnableRateLimiting(RateLimitPolicies.Reads)]
    [HttpGet("{offerId}")]
    [Authorize]
    public async Task<ActionResult<ShopOfferDto>> GetOfferById(string offerId)
    {
        var response = await shopService.GetOfferById(offerId);
        return Ok(response);
    }

    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [HttpPost("buy")]
    [Authorize]
    public async Task<ActionResult> BuyOffer([FromBody] BuyOfferRequest request)
    {
        await shopService.BuyOffer(request);
        return Ok();
    }
}
