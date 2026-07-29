using Nexus.Features.Inventory.Dto;

namespace Nexus.Features.Shop.Dto;

public class ShopOfferDto
{
    public string OfferId { get; set; }
    public CurrencyType PriceCurrency { get; set; }
    public int PriceAmount { get; set; }
    public string RewardItemId { get; set; }
    public int RewardAmount { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
}
