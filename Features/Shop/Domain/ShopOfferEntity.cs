using Microsoft.EntityFrameworkCore;
using Nexus.Features.Inventory.Dto;
using Nexus.Features.Shop.Dto;

namespace Nexus.Features.Shop.Domain;

[Index(nameof(OfferId), IsUnique = true)]
public class ShopOfferEntity
{
    public Guid Id { get; set; }
    public string OfferId { get; set; }
    public CurrencyType PriceCurrency { get; set; }
    public int PriceAmount { get; set; }
    public string RewardItemId { get; set; }
    public int RewardAmount { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
}
