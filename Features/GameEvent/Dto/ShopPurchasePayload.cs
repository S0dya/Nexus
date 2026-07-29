using Nexus.Features.Inventory.Dto;

namespace Nexus.Features.Shop.Dto;

public class ShopPurchasePayload
{
    public string OfferId { get; set; }
    public CurrencyType CurrencyType { get; set; }
    public int CurrencySpent { get; set; }
    public string ItemId { get; set; }
    public int ItemAmount { get; set; }
}