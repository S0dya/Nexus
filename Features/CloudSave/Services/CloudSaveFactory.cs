using Microsoft.Extensions.Options;
using Nexus.Features.CloudSave.Domain;
using Nexus.Options;

namespace Nexus.Features.CloudSave.Services;

public class CloudSaveFactory(IOptions<CloudSaveOptions> cloudSaveOptions) : ICloudSaveFactory
{
    private readonly CloudSaveOptions _cloudSaveOptions = cloudSaveOptions.Value;
    
    public CloudSaveEntity CreateCloudSave(Guid userId)
    {
        return new CloudSaveEntity
        {
            UserId = userId,
            Data = _cloudSaveOptions.DefaultSaveData,
            Version = 1,
            UpdatedAt = DateTime.UtcNow,
        };
    }
}
