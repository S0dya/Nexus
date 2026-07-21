using Nexus.Features.Auth.CurrentUser;
using Nexus.Features.Auth.Jwt;
using Nexus.Features.Auth.Services;
using Nexus.Features.Auth.Validation;
using Nexus.Features.CloudSave.Services;
using Nexus.Features.Leaderboard.Services;
using Nexus.Features.Profile.Services;
using Nexus.Features.Registration.Services;
using Nexus.Infrastructure.Security;
using Nexus.Options;

namespace Nexus.Infrastructure.DependencyInjection;

public static class ProjectInjection
{
    public static IServiceCollection AddProjectServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<DeviceOptions>(configuration.GetSection("DeviceSettings"));
        services.Configure<ProjectSettingsOptions>(configuration.GetSection("ProjectSettings"));
        services.Configure<ProfileOptions>(configuration.GetSection("ProfileSettings"));

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IPasswordValidation, PasswordValidation>();
        services.AddScoped<IDeviceFactory, DeviceFactory>();
        services.AddScoped<ICloudSaveFactory, CloudSaveFactory>();
        
        services.AddScoped<IDbAuthService, DbAuthService>();
        services.AddScoped<IProfileService, DbProfileService>();
        services.AddScoped<IAccountRegistrationService, DbAccountRegistrationService>();
        services.AddScoped<IUserActivityService, DbUserActivityService>();
        services.AddScoped<ICloudSaveService, DbCloudSaveService>();
        services.AddScoped<ILeaderboardService, DbLeaderboardService>();
        
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();

        return services;
    }
}
