using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Nexus.Database;
using Nexus.Features.Auth.CurrentUser;
using Nexus.Features.CloudSave.Dto;
using Nexus.Infrastructure.Exceptions;
using Nexus.Options;

namespace Nexus.Features.CloudSave.Services;

public class DbCloudSaveService(ILogger<DbCloudSaveService> logger, 
    ICurrentUser currentUser,
    AppDbContext db,
    IOptions<CloudSaveOptions> cloudSaveOptions,
    ICloudSaveFactory cloudSaveFactory) : ICloudSaveService
{
    private readonly CloudSaveOptions _cloudSaveOptions = cloudSaveOptions.Value;
    
    public async Task<SaveDataResponse> SaveData(SaveDataRequest request)
    {
        var saveEntity = await db.CloudSaves.FirstOrDefaultAsync(s => s.UserId == currentUser.UserId);

        if (saveEntity == null)
        {
            logger.LogWarning("Save not found for {UserId}", currentUser.UserId);
            // saveEntity = cloudSaveFactory.CreateCloudSave(currentUser.UserId);
            // await db.SaveChangesAsync();
            throw new NotFoundException("Save not found");
        }

        if (saveEntity.Version != request.Version)
        {
            logger.LogWarning("Save conflict for {UserId}. Server version: {ServerVersion}, client version: {ClientVersion}",
                currentUser.UserId,
                saveEntity.Version,
                request.Version);
            throw new ConflictException("Save version mismatch");        
        }
        
        saveEntity.Data = request.Data;
        saveEntity.Version++;
        saveEntity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        
        return new SaveDataResponse
        {
            Version = saveEntity.Version
        };
    }

    public async Task<LoadDataResponse> LoadData()
    {
        var saveEntity = await db.CloudSaves.FirstOrDefaultAsync(s => s.UserId == currentUser.UserId);

        if (saveEntity == null)
        {
            logger.LogWarning("Loading a save not found for {UserId}", currentUser.UserId);
            throw new NotFoundException("Save not found");
        }
        
        return new LoadDataResponse
        {
            Version = saveEntity.Version,
            Data = saveEntity.Data
        };
    }

    public async Task ResetSave()
    {
        var saveEntity = await db.CloudSaves.FirstOrDefaultAsync(s => s.UserId == currentUser.UserId);

        if (saveEntity == null)
        {
            logger.LogWarning("Trying to delete a save not found for {UserId}", currentUser.UserId);
            throw new NotFoundException("Save not found");
        }

        saveEntity.Data = _cloudSaveOptions.DefaultSaveData;
        saveEntity.Version++;
        saveEntity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }
}
