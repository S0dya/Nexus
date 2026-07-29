namespace Nexus.Features.Inventory.Dto;

public class GetInventoryResponse
{
    public List<InventoryItemDto> Items { get; set; } = new();
    public List<InventoryCurrencyDto> Currencies { get; set; } = new();
}
