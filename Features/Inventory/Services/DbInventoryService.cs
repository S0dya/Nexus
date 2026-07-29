using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexus.Database;
using Nexus.Features.Auth.CurrentUser;
using Nexus.Features.Inventory.Domain;
using Nexus.Features.Inventory.Dto;
using Nexus.Infrastructure.Exceptions;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace Nexus.Features.Inventory.Services;

public class DbInventoryService(ILogger<DbInventoryService> logger, 
    ICurrentUser currentUser,
    AppDbContext db) : IInventoryService
{
    public async Task<GetInventoryResponse> GetInventory()
    {
        logger.LogInformation("Fetching inventory for user {UserId}", currentUser.UserId);
        var inventory = await GetInventoryEntityOrThrow();

        var items = await db.InventoryItems
            .Where(x => x.UserId == currentUser.UserId)
            .Select(x => new InventoryItemDto
            {
                ItemId = x.ItemId,
                Amount = x.Amount
            })
            .ToListAsync();
        
        var currencies = new List<InventoryCurrencyDto>()
        {
            new()
            {
                CurrencyType = CurrencyType.Coins, 
                Amount = inventory.Coins
            },
            new()
            {
                CurrencyType = CurrencyType.Gems, 
                Amount = inventory.Gems
            },
        };
        
        return new GetInventoryResponse
        {
            Items = items,
            Currencies = currencies
        };
    }

    public async Task GrantCurrency(CurrencyType currencyType, int amount)
    {
        logger.LogInformation("Granting {Amount} {CurrencyType} to user {UserId}", amount, currencyType, currentUser.UserId);
        var inventory = await GetInventoryEntityOrThrow();

        if (amount <= 0)
        {
            throw new ValidationException("Amount must be positive");
        }

        var absAmount = Math.Abs(amount);
        await ChangeCurrency(inventory, currencyType, absAmount);
    }

    public async Task SpendCurrency(CurrencyType currencyType, int amount)
    {
        logger.LogInformation("Spending {Amount} {CurrencyType} for user {UserId}", amount, currencyType, currentUser.UserId);
        var inventory = await GetInventoryEntityOrThrow();

        switch (currencyType)
        {
            case CurrencyType.Coins: if (inventory.Coins - amount < 0) throw new ValidationException("Not enough coins"); break;
            case CurrencyType.Gems: if (inventory.Gems - amount < 0) throw new ValidationException("Not enough gems"); break;
            
            default: throw new ValidationException("Invalid currency type");
        }
        
        var absAmount = -Math.Abs(amount);
        await ChangeCurrency(inventory, currencyType, absAmount);
    }
    
    private async Task<InventoryEntity> GetInventoryEntityOrThrow()
    {
        var inventory = await db.Inventory.FirstOrDefaultAsync(x => x.UserId == currentUser.UserId);
        
        if (inventory == null)
        {
            logger.LogWarning("Inventory not found for user {UserId}", currentUser.UserId);
            throw new NotFoundException("Inventory not found");
        }
        
        return inventory;
    }
    
    private async Task ChangeCurrency(InventoryEntity inventory, CurrencyType type, int amount)
    {
        logger.LogDebug("Changing currency: {CurrencyType} by {Amount}", type, amount);
        
        switch (type)
        {
            case CurrencyType.Coins: inventory.Coins += amount; break;
            case CurrencyType.Gems: inventory.Gems += amount; break;
            
            default: throw new ValidationException("Invalid currency type");
        }
    }

    
    public async Task GrantItem(string itemId, int amount)
    {
        logger.LogInformation("Granting {Amount} of item {ItemId} to user {UserId}", amount, itemId, currentUser.UserId);
        var inventoryItem = await GetInventoryItemEntityOrCreate(itemId);

        if (amount <= 0)
        {
            throw new ValidationException("Amount must be positive");
        }
        
        inventoryItem.Amount += amount;
    }

    public async Task RemoveItem(string itemId, int amount)
    {
        logger.LogInformation("Removing {Amount} of item {ItemId} from user {UserId}", amount, itemId, currentUser.UserId);
        var inventoryItem = await GetInventoryItemEntityOrCreate(itemId);
        
        if (amount <= 0)
        {
            throw new ValidationException("Amount must be positive");
        }
        
        if (inventoryItem.Amount - amount < 0)
        {
            throw new ValidationException("Not enough items");
        }

        inventoryItem.Amount -= amount;
        
        if (inventoryItem.Amount == 0)
        {
            db.InventoryItems.Remove(inventoryItem);
        }
    }

    public async Task<HasItemResponse> HasItem(string itemId)
    {
        logger.LogDebug("Checking if user {UserId} has item {ItemId}", currentUser.UserId, itemId);
        var inventoryItem = await db.InventoryItems.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == currentUser.UserId && x.ItemId == itemId);

        var hasItem = inventoryItem != null && inventoryItem.Amount > 0;
        var amount = hasItem ? inventoryItem.Amount : 0;

        return new HasItemResponse
        {
            HasItem = hasItem,
            Amount = amount
        };
    }

    private async Task<InventoryItemEntity> GetInventoryItemEntityOrCreate(string itemId)
    {
        var inventoryItem = await db.InventoryItems.FirstOrDefaultAsync(x => x.UserId == currentUser.UserId && x.ItemId == itemId);
        
        if (inventoryItem == null)
        {
            logger.LogWarning("Inventory Item {ItemId} not found for user {UserId}", 
                itemId, currentUser.UserId);

            inventoryItem = new InventoryItemEntity()
            {
                UserId = currentUser.UserId,
                ItemId = itemId,
                Amount = 0,
            };
            
            db.InventoryItems.Add(inventoryItem);
        }
        
        return inventoryItem;
    }

    public async Task Commit()
    {
        await db.SaveChangesAsync();
    }
}
