using System.Security.Claims;
using System.Threading.RateLimiting;

namespace Nexus.Infrastructure.DependencyInjection.RateLimiting;

public static class RateLimitInjection
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "global",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            options.AddPolicy(RateLimitPolicies.Auth, context =>
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetIp(context),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    });
            });

            options.AddPolicy(RateLimitPolicies.Reads, context =>
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetUserId(context),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 30,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    });
            });

            options.AddPolicy(RateLimitPolicies.Writes, context =>
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetUserId(context),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    });
            });

            options.AddPolicy(RateLimitPolicies.History, context =>
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetUserId(context),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 15,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    });
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                
                await context.HttpContext.Response.WriteAsync(
                    "Too many requests",
                    cancellationToken);
            };
        });

        return services;

        string GetUserId(HttpContext context)
        {
            return context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? GetIp(context);
        }

        string GetIp(HttpContext context)
        {
            return context.Connection.RemoteIpAddress?.ToString()
                   ?? "unknown";
        }
    }
}