namespace Nexus.Features.Profile.Services;

public interface IUserActivityService
{
    public Task UpdateLastOnline(Guid userId);
}