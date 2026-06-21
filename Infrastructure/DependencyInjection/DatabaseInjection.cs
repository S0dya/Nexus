using Microsoft.EntityFrameworkCore;
using Nexus.Database;

namespace Nexus.Infrastructure.DependencyInjection;

public static class DatabaseInjection
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(
            configuration.GetConnectionString("DefaultConnection")));

        return services;
    }
}
