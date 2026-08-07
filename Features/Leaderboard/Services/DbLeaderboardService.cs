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
    IOptions<LeaderboardOptions> leaderboardOptions,
    ILeaderboardCache cache) : ILeaderboardService
{
    private readonly LeaderboardOptions _leaderboardOptions = leaderboardOptions.Value;
    
    public async Task<SubmitScoreResponse> SubmitScore(SubmitScoreRequest request)
    {
        logger.LogInformation("SubmitScore attempt for user {UserId} with score {Score}", currentUser.UserId, request.Score);

        if (request.Score < 0)
        {
            logger.LogWarning("SubmitScore failed for user {UserId}: score cannot be negative", currentUser.UserId);
            throw new ArgumentException("Score cannot be negative");
        }
        
        var entry = await db.LeaderboardEntryEntities.FirstOrDefaultAsync(entry => entry.UserId == currentUser.UserId);

        if (entry == null)
        {
            logger.LogInformation(
                "Creating new leaderboard entry for user {UserId} with score {Score}",
                currentUser.UserId,
                request.Score);

            var newEntry = new LeaderboardEntryEntity()
            {
                UserId = currentUser.UserId,
                BestScore = request.Score,
                LastUpdated = DateTime.UtcNow,
            };

            db.LeaderboardEntryEntities.Add(newEntry);
            await db.SaveChangesAsync();

            await cache.InvalidateGlobalLeaderboard();

            return new SubmitScoreResponse
            {
            };
        }
        
        if (entry.BestScore < request.Score)
        {
            logger.LogInformation("Updating best score for user {UserId} from {OldScore} to {NewScore}", 
                currentUser.UserId, entry.BestScore, request.Score);

            entry.BestScore = request.Score;
            entry.LastUpdated = DateTime.UtcNow;
            await db.SaveChangesAsync();
            
            await cache.InvalidateGlobalLeaderboard();
        }
        else
        {
            logger.LogDebug("Score {Score} not better than existing best score {BestScore} for user {UserId}", 
                request.Score, entry.BestScore, currentUser.UserId);
        }
        
        return new SubmitScoreResponse
        {
        };
    }

    public async Task<GlobalLeaderboardResponse> GetGlobalLeaderboard(GlobalLeaderboardRequest request)
    {
        logger.LogInformation("GetGlobalLeaderboard request with offset {Offset} and limit {Limit}", request.Offset, request.Limit);

        if (request.Offset < 0) throw new ArgumentException("Offset cannot be negative");
        if (request.Limit < 0 || request.Limit > _leaderboardOptions.GlobalLeaderboardLimitMaxValue)
            request.Limit = _leaderboardOptions.GlobalLeaderboardLimitMaxValue;

        var cachedResponse = await cache.TryGetGlobalLeaderboard(request.Offset, request.Limit);

        if (cachedResponse != null)
        {
            logger.LogInformation("Returning cached leaderboard entries");
            
            return cachedResponse;
        }
        
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
        
        logger.LogInformation("Returning {Count} leaderboard entries", entries.Count);

        var globalLeaderboardResponse = new GlobalLeaderboardResponse
        {
            Entries = entries
        };
        
        await cache.SetGlobalLeaderboard(request.Offset, request.Limit, globalLeaderboardResponse);
        
        return globalLeaderboardResponse;
    }

    public async Task<MyLeaderboardResponse> GetMyLeaderboard()
    {
        logger.LogInformation("GetMyLeaderboard request for user {UserId}", currentUser.UserId);

        var userEntry = await db.LeaderboardEntryEntities.FirstOrDefaultAsync(entry => entry.UserId == currentUser.UserId);
        
        if (userEntry == null)
        {
            logger.LogWarning("Leaderboard entry not found for user {UserId}", currentUser.UserId);

            return new MyLeaderboardResponse
            {
                Rank = null,
                BestScore = 0
            };
        }
        
        //fix needed in the future, for cases as User A - 500, User B - 500
        var count = await db.LeaderboardEntryEntities.CountAsync(entry => entry.BestScore > userEntry.BestScore);
        
        logger.LogInformation("User {UserId} has rank {Rank} with best score {BestScore}", 
            currentUser.UserId, count + 1, userEntry.BestScore);

        return new MyLeaderboardResponse
        {
            Rank = count + 1,
            BestScore = userEntry.BestScore
        };
    }
}
