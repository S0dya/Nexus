using Nexus.Features.CloudSave.Domain;

namespace Nexus.Features.CloudSave.Services;

public interface ICloudSaveFactory
{
    CloudSaveEntity CreateCloudSave(Guid userId);
}
