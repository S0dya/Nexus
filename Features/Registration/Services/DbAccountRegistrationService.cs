using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Nexus.Database;
using Nexus.Features.Auth.Domain;
using Nexus.Features.Auth.Dto;
using Nexus.Features.Auth.Jwt;
using Nexus.Features.Auth.Services;
using Nexus.Features.CloudSave.Domain;
using Nexus.Features.CloudSave.Services;
using Nexus.Features.Inventory.Services;
using Nexus.Features.Profile.Domain;
using Nexus.Features.Profile.Dto;
using Nexus.Features.Registration.Dto;
using Nexus.Options;

namespace Nexus.Features.Registration.Services;

public class DbAccountRegistrationService(
    IOptions<DeviceOptions> deviceOptions,
    IOptions<ProfileOptions> profileOptions,
    AppDbContext db,
    ILogger<DbAccountRegistrationService> logger,
    IDeviceFactory deviceFactory,
    ICloudSaveFactory cloudSaveFactory,
    IInventoryFactory inventoryFactory
    ) : IAccountRegistrationService
{
    private readonly DeviceOptions _deviceOptions = deviceOptions.Value;
    private readonly ProfileOptions _profileOptions = profileOptions.Value;
    
    public async Task<AccountRegistrationResult> CreateAccount(AccountRegistrationRequest request)
    {
        logger.LogInformation("Account creation attempt");
        
        var newUser = new UserEntity
        {
            Id = Guid.NewGuid(),
            UserEmail = request.UserEmail,
            Username = request.Username,
            PasswordHash = request.PasswordHash,
            UserRole = request.UserRole,
            CreatedAt = DateTime.UtcNow,
        };
        
        var newDevice = deviceFactory.CreateDevice(newUser.Id, request.DeviceId, _deviceOptions.ExpiryDays);
        
        var newProfile = new ProfileEntity
        {
            UserId = newUser.Id,
            DisplayName = "",
            IconId = _profileOptions.DefaultIconId,
            Bio = "",
            Country = null,
            Level = 0,
            Experience = 0,
            CreatedAt = DateTime.UtcNow,
            LastOnline = DateTime.UtcNow,
        };

        var newCloudSave = cloudSaveFactory.CreateCloudSave(newUser.Id);

        var newInventory = inventoryFactory.CreateInventory(newUser.Id);

        db.Users.Add(newUser);
        db.Profiles.Add(newProfile);
        db.Devices.Add(newDevice);
        db.CloudSaves.Add(newCloudSave);
        db.Inventory.Add(newInventory);
        await db.SaveChangesAsync();
        
        logger.LogInformation("Account creation succeeded for user {UserId}", newUser.Id);
        
        return new AccountRegistrationResult()
        {
            User = newUser,
            Device = newDevice
        };
    }
}