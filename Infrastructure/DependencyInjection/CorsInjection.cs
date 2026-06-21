namespace Nexus.Infrastructure.DependencyInjection;

public static class CorsInjection
{
    public static IServiceCollection TimerAddCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }

    public static IApplicationBuilder TimerUseCors(this IApplicationBuilder app)
    {
        app.UseCors("AllowAll");
        return app;
    }
}
