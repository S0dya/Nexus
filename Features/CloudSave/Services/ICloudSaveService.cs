using Nexus.Features.CloudSave.Dto;

namespace Nexus.Features.CloudSave.Services;

public interface ICloudSaveService
{
    Task<SaveDataResponse> SaveData(SaveDataRequest request);
    Task<LoadDataResponse> LoadData();
    Task ResetSave();
}
