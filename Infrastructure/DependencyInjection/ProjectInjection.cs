using Nexus.Features.Auth.CurrentUser;
using Nexus.Features.Auth.Jwt;
using Nexus.Features.Auth.Services;
using Nexus.Features.Auth.Validation;
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

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IDbAuthService, DbAuthService>();
        services.AddScoped<IPasswordValidation, PasswordValidation>();
        
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();

        return services;
    }
}
