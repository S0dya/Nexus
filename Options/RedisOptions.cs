namespace Nexus.Options;

public class RedisOptions
{
    public const string SectionName = "Redis";
    public string ConnectionString { get; set; } = "localhost:6379";
    public int DatabaseId { get; set; } = 0;
    public int LeaderboardCacheSeconds { get; set; } = 30;
}
