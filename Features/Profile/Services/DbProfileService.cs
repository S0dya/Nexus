using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Nexus.Database;
using Nexus.Features.Auth.CurrentUser;
using Nexus.Features.Profile.Dto;
using Nexus.Infrastructure.Exceptions;
using Nexus.Options;

namespace Nexus.Features.Profile.Services;

public class DbProfileService(
    ILogger<DbProfileService> logger,
    AppDbContext db,
    ICurrentUser currentUser,
    IOptions<ProfileOptions> profileOptions)
    : IProfileService
{
    private readonly ProfileOptions _profileOptions = profileOptions.Value;
    
    public async Task<ProfileResponse> GetUser(Guid userId)
    {
        logger.LogInformation("Get User profile attempt for {userId}", userId);

        var existingProfile = await db.Profiles.FirstOrDefaultAsync(profile => profile.UserId == userId);
        
        if (existingProfile == null)
        {
            throw new NotFoundException("Profile not found");
        }
        
        var profileResponse = new ProfileResponse()
        {
            Name = existingProfile.DisplayName,
            IconId = existingProfile.IconId,
            Bio = existingProfile.Bio,
            Country = existingProfile.Country,
            LastOnline = existingProfile.LastOnline,
            Level = existingProfile.Level,
        };

        return profileResponse;
    }

    public async Task PatchProfile(PatchProfileRequest request)
    {
        var currentUserId = currentUser.UserId;
        
        logger.LogInformation("Patch profile attempt for {userId}", currentUserId);

        var existingProfile = await db.Profiles.FirstOrDefaultAsync(profile => profile.UserId == currentUserId);
        
        if (existingProfile == null)
        {
            throw new NotFoundException("Profile not found");
        }

        if (request.Name != null)
        {
            //validation
            
            existingProfile.DisplayName = request.Name;
        }
        if (request.IconId != null)
        {
            //validation

            if (request.IconId < 0)
            {
                logger.LogWarning("Negative IconId patch for {userId}", currentUserId);
                throw new ValidationException("Icon id cannot be negative");
            }
            if (request.IconId > _profileOptions.MaxIconId)
            {
                logger.LogWarning("IconId exceeds maximum for {userId}", currentUserId);
                throw new ValidationException("Icon id exceeds maximum allowed value");
            }
            
            existingProfile.IconId = request.IconId.Value;
        }

        if (request.Bio != null)
        {
            //validation
            
            existingProfile.Bio = request.Bio;
        }

        if (request.Country != null)
        {
            //validation
            
            existingProfile.Country = request.Country;
        }
        
        // if (changesMade)
        await db.SaveChangesAsync(); 
        
        // return fullProfile?
    }

    public async Task<FullProfileResponse> GetMe()
    {
        var currentUserId = currentUser.UserId;
        
        logger.LogInformation("Get Me attempt for {userId}", currentUserId);
        
        var existingProfile = await db.Profiles.FirstOrDefaultAsync(profile => profile.UserId == currentUserId);

        if (existingProfile == null)
        {
            throw new NotFoundException("Profile not found");
        }

        var profileResponse = new FullProfileResponse()
        {
            Name = existingProfile.DisplayName,
            IconId = existingProfile.IconId,
            Bio = existingProfile.Bio,
            Country = existingProfile.Country,
            LastOnline = existingProfile.LastOnline,
            Level = existingProfile.Level,
            Experience = existingProfile.Experience,
            CreatedAt = existingProfile.CreatedAt,
        };

        return profileResponse;
    }
}
