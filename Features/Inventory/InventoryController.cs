using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Nexus.Features.Inventory.Dto;
using Nexus.Features.Inventory.Services;
using Nexus.Infrastructure.DependencyInjection.RateLimiting;

namespace Nexus.Features.Inventory;

[ApiController]
[Route("inventory")]
public class InventoryController(IInventoryService inventoryService) : ControllerBase
{
    [EnableRateLimiting(RateLimitPolicies.Reads)]
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<GetInventoryResponse>> GetInventory()
    {
        var response = await inventoryService.GetInventory();
        return Ok(response);
    }

    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [HttpPost("grant-currency")]
    [Authorize]
    public async Task<ActionResult> GrantCurrency([FromBody] CurrencyRequest request)
    {
        await inventoryService.GrantCurrency(request.CurrencyType, request.Amount);
        return Ok();
    }

    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [HttpPost("spend-currency")]
    [Authorize]
    public async Task<ActionResult> SpendCurrency([FromBody] CurrencyRequest request)
    {
        await inventoryService.SpendCurrency(request.CurrencyType, request.Amount);
        return Ok();
    }

    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [HttpPost("grant-item")]
    [Authorize]
    public async Task<ActionResult> GrantItem([FromBody] GrantItemRequest request)
    {
        await inventoryService.GrantItem(request.ItemId, request.Amount);
        await inventoryService.Commit();
        return Ok();
    }

    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [HttpPost("remove-item")]
    [Authorize]
    public async Task<ActionResult> RemoveItem([FromBody] RemoveItemRequest request)
    {
        await inventoryService.RemoveItem(request.ItemId, request.Amount);
        await inventoryService.Commit();
        return Ok();
    }

    [EnableRateLimiting(RateLimitPolicies.Reads)]
    [HttpPost("has-item")]
    [Authorize]
    public async Task<ActionResult<HasItemResponse>> HasItem([FromBody] HasItemRequest request)
    {
        var response = await inventoryService.HasItem(request.ItemId);
        return Ok(response);
    }
}
