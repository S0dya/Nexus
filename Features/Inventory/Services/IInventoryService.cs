using Nexus.Features.Inventory.Dto;

namespace Nexus.Features.Inventory.Services;

public interface IInventoryService
{
    Task<GetInventoryResponse> GetInventory();
    Task GrantCurrency(CurrencyType currencyType, int amount);
    Task SpendCurrency(CurrencyType currencyType, int amount);
    Task GrantItem(string itemId, int amount);
    Task RemoveItem(string itemId, int amount);
    Task<HasItemResponse> HasItem(string itemId);
    Task Commit();
}

