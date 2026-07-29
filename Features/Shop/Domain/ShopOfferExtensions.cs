using Nexus.Features.Shop.Dto;

namespace Nexus.Features.Shop.Domain;

public static class ShopOfferExtensions
{
    public static ShopOfferDto ToDto(this ShopOfferEntity entity)
    {
        return new ShopOfferDto
        {
            OfferId = entity.OfferId,
            PriceCurrency = entity.PriceCurrency,
            PriceAmount = entity.PriceAmount,
            RewardItemId = entity.RewardItemId,
            RewardAmount = entity.RewardAmount,
            IsEnabled = entity.IsEnabled,
            StartsAt = entity.StartsAt,
            EndsAt = entity.EndsAt
        };
    }
}
