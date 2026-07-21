using Microsoft.EntityFrameworkCore;
using Nexus.Features.Auth.Domain;
using Nexus.Features.CloudSave.Domain;
using Nexus.Features.Leaderboard.Domain;
using Nexus.Features.Profile.Domain;

namespace Nexus.Database;

public class AppDbContext : DbContext
{
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<DeviceEntity> Devices => Set<DeviceEntity>();
    public DbSet<ProfileEntity> Profiles => Set<ProfileEntity>();
    public DbSet<CloudSaveEntity> CloudSaves => Set<CloudSaveEntity>();
    public DbSet<LeaderboardEntryEntity> LeaderboardEntryEntities => Set<LeaderboardEntryEntity>();

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

    }
}