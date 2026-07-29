using Microsoft.EntityFrameworkCore;
using Nexus.Database;
using Nexus.Features.Analytics.Services;
using Nexus.Features.Auth.CurrentUser;
using Nexus.Features.GameEvent.Dto;
using Nexus.Features.GameEvent.Services;
using Nexus.Features.Inventory.Dto;
using Nexus.Features.Inventory.Services;
using Nexus.Features.Shop.Domain;
using Nexus.Features.Shop.Dto;
using Nexus.Infrastructure.Exceptions;

namespace Nexus.Features.Shop.Services;

public class DbShopService(AppDbContext db, 
    ICurrentUser currentUser,
    IInventoryService inventoryService,
    ILogger<DbShopService> logger,
    IGameEventService gameEventService) : IShopService
{
    public async Task<GetShopOffersResponse> GetAllOffers()
    {
        logger.LogInformation("Fetching all shop offers");
        var offers = await db.ShopOffers
            .Where(x => x.IsEnabled && 
                        (x.StartsAt == null || x.StartsAt <= DateTime.UtcNow) && 
                        (x.EndsAt == null || x.EndsAt >= DateTime.UtcNow))
            .Select(x => x.ToDto())
            .ToListAsync();

        return new GetShopOffersResponse()
        {
            Offers = offers
        };
    }

    public async Task<ShopOfferDto> GetOfferById(string offerId)
    {
        logger.LogInformation("Fetching shop offer {OfferId}", offerId);
        var offer = await GetOfferOrThrow(offerId);

        return offer.ToDto();
    }

    public async Task BuyOffer(BuyOfferRequest request)
    {
        logger.LogInformation("User {UserId} attempting to buy offer {OfferId}", currentUser.UserId, request.OfferId);
        var offer = await GetOfferOrThrow(request.OfferId);
        
        if (!offer.IsEnabled) 
            throw new ValidationException("Offer is not enabled or is out of date");
        if (offer.EndsAt.HasValue && offer.EndsAt < DateTime.UtcNow)
            throw new ValidationException("Offer is expired");
        if (offer.StartsAt.HasValue && offer.StartsAt > DateTime.UtcNow)
            throw new ValidationException("Offer is not active yet");
        
        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            await inventoryService.SpendCurrency(offer.PriceCurrency, offer.PriceAmount);

            await inventoryService.GrantItem(offer.RewardItemId, offer.RewardAmount);

            db.InventoryTransactions.Add(new()
            {
                 Id = Guid.NewGuid(),
                 UserId = currentUser.UserId,
                 Reason = InventoryTransactionReason.ShopPurchase,
                 ItemId = offer.RewardItemId,
                 ItemAmount = offer.RewardAmount,
                 ReferenceId = offer.OfferId,
                 CreatedAt = DateTime.UtcNow,
            });
            db.InventoryTransactions.Add(new()
            {
                 Id = Guid.NewGuid(),
                 UserId = currentUser.UserId,
                 Reason = InventoryTransactionReason.ShopPurchase,
                 CurrencyType = offer.PriceCurrency,
                 CurrencyAmount = offer.PriceAmount,
                 ReferenceId = offer.OfferId,
                 CreatedAt = DateTime.UtcNow,
            });
            
            logger.LogInformation("User {UserId} successfully bought offer {OfferId} - Spent {Amount} {CurrencyType}", 
                currentUser.UserId, request.OfferId, offer.PriceAmount, offer.PriceCurrency);
            
            await gameEventService.AddEvent(GameEventType.ShopPurchase,
                new ShopPurchasePayload
                {
                    OfferId = offer.OfferId,
                    CurrencyType = offer.PriceCurrency,
                    CurrencySpent = offer.PriceAmount,
                    ItemId = offer.RewardItemId,
                    ItemAmount = offer.RewardAmount
                });
            
            await inventoryService.Commit();
            
            await transaction.CommitAsync();
        }
        catch
        {
            logger.LogError("Failed to buy offer {OfferId} for user {UserId}, rolling back transaction", request.OfferId, currentUser.UserId);
            await transaction.RollbackAsync();

            throw;
        }
    }

    private async Task<ShopOfferEntity> GetOfferOrThrow(string offerId)
    {
        var offer = await db.ShopOffers.FirstOrDefaultAsync(x => x.OfferId == offerId);

        if (offer == null)
        {
            throw new NotFoundException("Offer not found");
        }

        return offer;
    }
}
