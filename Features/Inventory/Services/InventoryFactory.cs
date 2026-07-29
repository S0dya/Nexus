using Microsoft.Extensions.Options;
using Nexus.Features.Inventory.Domain;
using Nexus.Options;

namespace Nexus.Features.Inventory.Services;

public class InventoryFactory(IOptions<InventoryOptions> inventoryOptions) : IInventoryFactory
{
    private readonly InventoryOptions _inventoryOptions = inventoryOptions.Value;
    
    public InventoryEntity CreateInventory(Guid userId)
    {
        return new InventoryEntity
        {
            UserId = userId,
            Coins = _inventoryOptions.DefaultCoins,
            Gems = _inventoryOptions.DefaultGems
        };
    }
}
