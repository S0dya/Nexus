using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Nexus.Database;
using Nexus.Features.Analytics.Domain;
using Nexus.Features.GameEvent.Domain;
using Nexus.Features.GameEvent.Dto;
using Nexus.Features.Shop.Dto;

namespace Nexus.Features.Analytics.Services;

public class DbAnalyticsService(AppDbContext db, 
    ILogger<DbAnalyticsService> logger) : IAnalyticsService
{

    public async Task ProcessEvent(GameEventEntity gameEvent)
    {
        var analytics = await db.PlayerAnalytics.FirstOrDefaultAsync(x => x.UserId == gameEvent.UserId);

        if (analytics == null)
        {
            analytics = new PlayerAnalyticsEntity
            {
                UserId = gameEvent.UserId
            };

            db.PlayerAnalytics.Add(analytics);
        }
        
        switch (gameEvent.Type)
        {
            case GameEventType.ShopPurchase:
                var payload = JsonSerializer.Deserialize<ShopPurchasePayload>(gameEvent.Payload);
                
                if (payload == null) 
                    throw new InvalidOperationException("Invalid payload");
                
                analytics.Purchases++;
                analytics.CoinsSpent += payload.CurrencySpent;
                analytics.ItemsBought += payload.ItemAmount;
                break;
            default: 
                throw new NotImplementedException($"Analytics doesn't support {gameEvent.Type}");
        }

        await db.SaveChangesAsync();
    }
}