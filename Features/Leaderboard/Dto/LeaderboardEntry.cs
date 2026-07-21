using System.ComponentModel.DataAnnotations;

namespace Nexus.Features.Leaderboard.Dto;

public class LeaderboardEntry
{
    public Guid UserId { get; set; }
    public string Username { get; set; }
    public int IconId { get; set; }
    public int BestScore { get; set; }
    public int Rank { get; set; }
}