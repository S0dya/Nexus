using Nexus.Database;
using Nexus.Features.Analytics.Services;
using Nexus.Features.Auth.CurrentUser;
using Nexus.Features.Auth.Jwt;
using Nexus.Features.Auth.Services;
using Nexus.Features.Auth.Validation;
using Nexus.Features.CloudSave.Services;
using Nexus.Features.GameEvent.Services;
using Nexus.Features.GameEvent.Workers;
using Nexus.Features.Inventory.Services;
using Nexus.Features.Leaderboard.Services;
using Nexus.Features.Profile.Services;
using Nexus.Features.Registration.Services;
using Nexus.Features.Shop.Services;
using Nexus.Infrastructure.Security;
using Nexus.Options;
using StackExchange.Redis;

namespace Nexus.Infrastructure.DependencyInjection;

public static class ProjectInjection
{
    public static IServiceCollection AddProjectServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<DeviceOptions>(configuration.GetSection("DeviceSettings"));
        services.Configure<ProjectSettingsOptions>(configuration.GetSection("ProjectSettings"));
        services.Configure<ProfileOptions>(configuration.GetSection("ProfileSettings"));
        services.Configure<InventoryOptions>(configuration.GetSection("InventorySettings"));
        services.Configure<GameEventProcessorOptions>(configuration.GetSection("GameEventProcessor"));

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IPasswordValidation, PasswordValidation>();
        services.AddScoped<IDeviceFactory, DeviceFactory>();
        services.AddScoped<ICloudSaveFactory, CloudSaveFactory>();
        services.AddScoped<IInventoryFactory, InventoryFactory>();
        services.AddScoped<IGameEventService, DbGameEventService>();
        services.AddScoped<IAnalyticsService, DbAnalyticsService>();
        
        services.AddScoped<IDbAuthService, DbAuthService>();
        services.AddScoped<IProfileService, DbProfileService>();
        services.AddScoped<IAccountRegistrationService, DbAccountRegistrationService>();
        services.AddScoped<IUserActivityService, DbUserActivityService>();
        services.AddScoped<ICloudSaveService, DbCloudSaveService>();
        services.AddScoped<IInventoryService, DbInventoryService>();
        services.AddScoped<ILeaderboardService, DbLeaderboardService>();
        services.AddScoped<IShopService, DbShopService>();
        
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();

        services.AddHostedService<GameEventProcessor>();
        
        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>()
            .AddRedis(sp => sp.GetRequiredService<IConnectionMultiplexer>());

        return services;
    }
}
