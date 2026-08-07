using System.Text.Json;
using StackExchange.Redis;
using Microsoft.Extensions.Options;
using Nexus.Features.Leaderboard.Dto;
using Nexus.Options;

namespace Nexus.Features.Leaderboard.Services;

public class RedisLeaderboardCache(
    IConnectionMultiplexer redis,
    ILogger<RedisLeaderboardCache> logger,
    IOptions<RedisOptions> redisOptions) : ILeaderboardCache
{
    private readonly IDatabase _db = redis.GetDatabase();
    private readonly RedisOptions _options = redisOptions.Value;
    
    private const string Key = "leaderboard";
    private const string VersionKey = "leaderboard:version";

    public async Task SetGlobalLeaderboard(int offset, int limit, GlobalLeaderboardResponse response)
    {
        var jitter = Random.Shared.NextDouble() * 0.2 - 0.1;
        var ttl = TimeSpan.FromSeconds(_options.LeaderboardCacheSeconds * (1 + jitter));
        
        var version = await GetVersion();
        
        await _db.StringSetAsync(GetKey(version, offset, limit), JsonSerializer.Serialize(response),ttl);
        
        logger.LogDebug("Cached leaderboard page {Offset}:{Limit} with version {Version}",
            offset, limit, version);
    }

    public async Task<GlobalLeaderboardResponse?> TryGetGlobalLeaderboard(int offset, int limit)
    {
        var version = await GetVersion();
        
        var redisValue = await _db.StringGetAsync(GetKey(version, offset, limit));
        
        if (!redisValue.HasValue)
        {
            logger.LogDebug("Leaderboard cache miss for {Offset}:{Limit}, version {Version}", 
                offset, limit, version);

            return null;
        }

        logger.LogDebug("Leaderboard cache hit for {Offset}:{Limit}, version {Version}", 
            offset, limit, version);
        
        

        return JsonSerializer.Deserialize<GlobalLeaderboardResponse>(redisValue.ToString());
    }
    
    public async Task InvalidateGlobalLeaderboard()
    {
        var newVersion = await _db.StringIncrementAsync(VersionKey);
        
        logger.LogDebug("Leaderboard cache invalidated. New version {Version}", newVersion);
    }
    
    private string GetKey(long version, int offset, int limit) => $"{Key}:{version}:{offset}:{limit}";

    private async Task<long> GetVersion()
    {
        var version = await _db.StringIncrementAsync(VersionKey, 0);
        
        return version;
    }
}