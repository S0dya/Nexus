using System.Text.Json;
using Microsoft.Extensions.Options;
using Nexus.Features.Leaderboard.Dto;
using Nexus.Options;
using StackExchange.Redis;

namespace Nexus.Features.Leaderboard.Services;

public class RedisSwrLeaderboardCache(
    IConnectionMultiplexer redis,
    ILogger<RedisSwrLeaderboardCache> logger,
    IOptions<RedisOptions> redisOptions) : ILeaderboardCache
{
    private readonly IDatabase _db = redis.GetDatabase();
    private readonly RedisOptions _redisOptions = redisOptions.Value;
    private readonly string _key = "leaderboard";

    public async Task SetGlobalLeaderboard(int offset, int limit, GlobalLeaderboardResponse response)
    {
        await _db.StringSetAsync(GetKey(offset, limit), JsonSerializer.Serialize(response),TimeSpan.FromSeconds(_redisOptions.LeaderboardCacheSeconds));
    }

    public async Task<GlobalLeaderboardResponse?> TryGetGlobalLeaderboard(int offset, int limit)
    {
        var redisValue = await _db.StringGetAsync(GetKey(offset, limit));

        // if (DateTime.UtcNow - redisValue.ExpiresAt < TimeSpan.FromSeconds(_redisOptions.LeaderboardCacheSeconds))
        // {
        //     //relead cache
        // } 
        
        return redisValue.HasValue ? JsonSerializer.Deserialize<GlobalLeaderboardResponse>(redisValue.ToString()) : null; 
    }
    
    public async Task DeleteGlobalLeaderboard()
    {
        await _db.KeyDeleteAsync(_key);
    }
    
    private string GetKey(int offset, int limit) => $"{_key}:{offset}:{limit}";
}