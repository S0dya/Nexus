using Microsoft.EntityFrameworkCore;
using Nexus.Features.Analytics.Domain;
using Nexus.Features.Auth.Domain;
using Nexus.Features.CloudSave.Domain;
using Nexus.Features.GameEvent.Domain;
using Nexus.Features.Inventory.Domain;
using Nexus.Features.Leaderboard.Domain;
using Nexus.Features.Profile.Domain;
using Nexus.Features.Shop.Domain;

namespace Nexus.Database;

public class AppDbContext : DbContext
{
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<DeviceEntity> Devices => Set<DeviceEntity>();
    public DbSet<ProfileEntity> Profiles => Set<ProfileEntity>();
    public DbSet<CloudSaveEntity> CloudSaves => Set<CloudSaveEntity>();
    public DbSet<LeaderboardEntryEntity> LeaderboardEntryEntities => Set<LeaderboardEntryEntity>();
    public DbSet<InventoryEntity> Inventory => Set<InventoryEntity>();
    public DbSet<InventoryItemEntity> InventoryItems => Set<InventoryItemEntity>();
    public DbSet<ShopOfferEntity> ShopOffers => Set<ShopOfferEntity>();
    public DbSet<InventoryTransactionEntity> InventoryTransactions => Set<InventoryTransactionEntity>();
    public DbSet<GameEventEntity> GameEvents => Set<GameEventEntity>();
    public DbSet<PlayerAnalyticsEntity> PlayerAnalytics => Set<PlayerAnalyticsEntity>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>()
            .HasKey(user => user.Id);

        modelBuilder.Entity<DeviceEntity>()
            .HasKey(device => device.Id);

        modelBuilder.Entity<ProfileEntity>()
            .HasKey(profile => profile.UserId);
        
        modelBuilder.Entity<CloudSaveEntity>()
            .HasKey(cloudSave => cloudSave.UserId);
        
        modelBuilder.Entity<LeaderboardEntryEntity>()
            .HasKey(leaderboardEntryEntity => leaderboardEntryEntity.UserId);

        modelBuilder.Entity<InventoryEntity>()
            .HasKey(inventory => inventory.UserId);

        modelBuilder.Entity<InventoryItemEntity>()
            .HasKey(item => new { item.UserId, item.ItemId });

        modelBuilder.Entity<ShopOfferEntity>()
            .HasKey(shopOffer => shopOffer.Id);
        
        modelBuilder.Entity<InventoryTransactionEntity>()
            .HasKey(transaction => transaction.Id);
        
        modelBuilder.Entity<GameEventEntity>()
            .HasKey(gameEvent => gameEvent.Id);
        
        modelBuilder.Entity<PlayerAnalyticsEntity>()
            .HasKey(playerAnalytics => playerAnalytics.UserId);
        
    }
}