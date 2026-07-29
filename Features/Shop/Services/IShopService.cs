using Nexus.Features.Shop.Dto;

namespace Nexus.Features.Shop.Services;

public interface IShopService
{
    Task<GetShopOffersResponse> GetAllOffers();
    Task<ShopOfferDto> GetOfferById(string offerId);
    Task BuyOffer(BuyOfferRequest request);
}
