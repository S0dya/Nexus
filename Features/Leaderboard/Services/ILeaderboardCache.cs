using Nexus.Features.Leaderboard.Dto;

namespace Nexus.Features.Leaderboard.Services;

public interface ILeaderboardCache
{
    Task SetGlobalLeaderboard(int offset, int limit, GlobalLeaderboardResponse response);
    Task<GlobalLeaderboardResponse?> TryGetGlobalLeaderboard(int offset, int limit);
    Task InvalidateGlobalLeaderboard();
}
