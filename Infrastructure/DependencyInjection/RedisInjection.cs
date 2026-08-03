using Microsoft.Extensions.Options;
using Nexus.Features.Leaderboard.Services;
using Nexus.Options;
using StackExchange.Redis;

namespace Nexus.Infrastructure.DependencyInjection;

public static class RedisInjection
{
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));
        
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<RedisOptions>>().Value;
            var configurationOptions = ConfigurationOptions.Parse(options.ConnectionString);
            configurationOptions.DefaultDatabase = options.DatabaseId;
            return ConnectionMultiplexer.Connect(configurationOptions);
        });
        
        services.AddScoped<ILeaderboardCache, RedisLeaderboardCache>();
        
        return services;
    }
}
