namespace Nexus.Infrastructure.DependencyInjection.RateLimiting;

public static class RateLimitPolicies
{
    public const string Auth = "Auth";
    public const string Reads = "Reads";
    public const string Writes = "Writes";
    public const string History = "History";
}