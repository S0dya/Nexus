using Nexus.Features.Leaderboard.Dto;

namespace Nexus.Features.Leaderboard.Services;

public interface ILeaderboardService
{
    Task<SubmitScoreResponse> SubmitScore(SubmitScoreRequest request);
    Task<GlobalLeaderboardResponse> GetGlobalLeaderboard(GlobalLeaderboardRequest request);
    Task<MyLeaderboardResponse> GetMyLeaderboard();
    Task ResetSeason();
}
