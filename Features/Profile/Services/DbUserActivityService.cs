using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Nexus.Database;
using Nexus.Options;

namespace Nexus.Features.Profile.Services;

public class DbUserActivityService(
    AppDbContext db, 
    ILogger<DbUserActivityService> logger,
    IOptions<ProfileOptions> profileOptions) : IUserActivityService
{
    private readonly ProfileOptions _profileOptions = profileOptions.Value; 
    
    public async Task UpdateLastOnline(Guid userId)
    {
        var profile = await db.Profiles.FirstOrDefaultAsync(x => x.UserId == userId);
        var now = DateTime.UtcNow;

        if (profile == null)
        {
            logger.LogError("Profile not found for userId {UserId}", userId);
            throw new InvalidOperationException("Profile not found");
        }
        
        if (now - profile.LastOnline < TimeSpan.FromMinutes(_profileOptions.LastOnlineUpdateIntervalMinutes))
        {
            return;
        }

        profile.LastOnline = now;
        await db.SaveChangesAsync();
    }
}