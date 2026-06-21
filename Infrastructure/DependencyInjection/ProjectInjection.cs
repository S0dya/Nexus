using Nexus.Features.Auth.CurrentUser;
using Nexus.Features.Auth.Jwt;
using Nexus.Features.Auth.Services;
using Nexus.Features.Auth.Validation;
using Nexus.Options;

namespace Nexus.Infrastructure.DependencyInjection;

public static class ProjectInjection
{
    public static IServiceCollection AddProjectServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<ProjectSettingsOptions>(configuration.GetSection("ProjectSettings"));

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPasswordValidation, PasswordValidation>();

        return services;
    }
}
