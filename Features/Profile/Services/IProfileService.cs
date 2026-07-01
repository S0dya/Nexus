using Nexus.Features.Profile.Dto;

namespace Nexus.Features.Profile.Services;

public interface IProfileService
{
    Task<ProfileResponse> GetUser(Guid userId);
    Task PatchProfile(PatchProfileRequest request);
    Task<FullProfileResponse> GetMe();
}
