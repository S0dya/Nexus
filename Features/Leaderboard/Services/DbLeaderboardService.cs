using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Nexus.Database;
using Nexus.Features.Auth.CurrentUser;
using Nexus.Features.Leaderboard.Domain;
using Nexus.Features.Leaderboard.Dto;
using Nexus.Options;

namespace Nexus.Features.Leaderboard.Services;

public class DbLeaderboardService(
    AppDbContext db,
    ILogger<DbLeaderboardService> logger,
    ICurrentUser currentUser,
    IOptions<LeaderboardOptions> leaderboardOptions) : ILeaderboardService
{
    private readonly LeaderboardOptions _leaderboardOptions = leaderboardOptions.Value;
    
    public async Task<SubmitScoreResponse> SubmitScore(SubmitScoreRequest request)
    {
        if (request.Score < 0)
        {
            throw new ArgumentException("Score cannot be negative");
        }
        
        var entry = await db.LeaderboardEntryEntities.FirstOrDefaultAsync(entry => entry.UserId == currentUser.UserId);

        if (entry == null)
        {
            var newEntry = new LeaderboardEntryEntity()
            {
                UserId = currentUser.UserId,
                BestScore = request.Score,
                LastUpdated = DateTime.UtcNow,
                // SeasonId = leaderboardOptions.Value.CurrentSeasonId
            };
            
            db.LeaderboardEntryEntities.Add(newEntry);
            await db.SaveChangesAsync();
            
            return new SubmitScoreResponse
            {
            };
        }
        
        if (entry.BestScore < request.Score)
        {
            entry.BestScore = request.Score;
            entry.LastUpdated = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
        
        return new SubmitScoreResponse
        {
            // BestScore = entry.BestScore
        };
    }

    public async Task<GlobalLeaderboardResponse> GetGlobalLeaderboard(GlobalLeaderboardRequest request)
    {
        if (request.Offset < 0) throw new ArgumentException("Offset cannot be negative");
        if (request.Limit < 0 || request.Limit > _leaderboardOptions.GlobalLeaderboardLimitMaxValue)
            request.Limit = _leaderboardOptions.GlobalLeaderboardLimitMaxValue;

        var entries = await db.LeaderboardEntryEntities
            .AsNoTracking()
            .OrderByDescending(x => x.BestScore)
            .ThenBy(x => x.LastUpdated)
            .Skip(request.Offset)
            .Take(request.Limit)
            .Join(
                db.Profiles,
                leaderboard => leaderboard.UserId,
                profile => profile.UserId,
                (leaderboard, profile) => new LeaderboardEntry
                {
                    UserId = leaderboard.UserId,
                    Username = profile.DisplayName,
                    IconId = profile.IconId,
                    BestScore = leaderboard.BestScore,
                })
            .ToListAsync();
        
        var rank = request.Offset + 1;
        foreach (var entry in entries)
        {
            entry.Rank = rank;
            rank++;
        }
        
        return new GlobalLeaderboardResponse
        {
            Entries = entries
        };
    }

    public async Task<MyLeaderboardResponse> GetMyLeaderboard()
    {
        var userEntry = await db.LeaderboardEntryEntities.FirstOrDefaultAsync(entry => entry.UserId == currentUser.UserId);
        
        if (userEntry == null)
        {
            return new MyLeaderboardResponse
            {
                Rank = null,
                BestScore = 0
            };
        }
        
        //fix needed in the future, for cases as User A - 500, User B - 500
        var count = await db.LeaderboardEntryEntities.CountAsync(entry => entry.BestScore > userEntry.BestScore);
        
        return new MyLeaderboardResponse
        {
            Rank = count + 1,
            BestScore = userEntry.BestScore
        };
    }

    public async Task ResetSeason()
    {
        
    }
}
