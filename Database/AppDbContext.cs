using Microsoft.EntityFrameworkCore;
using Nexus.Features.Auth.Domain;

namespace Nexus.Database;

public class AppDbContext : DbContext
{
    public DbSet<UserEntity> Users => Set<UserEntity>();
    
    public AppDbContext(DbContextOptions<AppDbContext> options) 
        : base(options)
    {
           
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>()
            .HasKey(user => user.Id);


    } 
}