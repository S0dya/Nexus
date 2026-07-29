using Nexus.Features.Inventory.Domain;

namespace Nexus.Features.Inventory.Services;

public interface IInventoryFactory
{
    InventoryEntity CreateInventory(Guid userId);
}
