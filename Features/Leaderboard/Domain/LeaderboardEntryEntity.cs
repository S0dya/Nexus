using Microsoft.EntityFrameworkCore;

namespace Nexus.Features.Leaderboard.Domain;

[Index(nameof(BestScore), nameof(LastUpdated))]
public class LeaderboardEntryEntity
{
    public Guid UserId { get; set; }

    public int BestScore { get; set; }

    public DateTime LastUpdated { get; set; }
}