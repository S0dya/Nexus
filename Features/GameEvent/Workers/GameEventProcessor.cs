using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Nexus.Database;
using Nexus.Features.Analytics.Services;
using Nexus.Options;

namespace Nexus.Features.GameEvent.Workers;

public class GameEventProcessor(
    ILogger<GameEventProcessor> logger,
    IServiceScopeFactory scopeFactory,
    IOptions<GameEventProcessorOptions> options) : BackgroundService
{
    private readonly GameEventProcessorOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("GameEventProcessor started with BatchSize={BatchSize}, Interval={Interval}s", 
            _options.BatchSize, _options.ProcessingIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();

                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var analyticsService = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();

                var events = await db.GameEvents
                    .Where(x => x.ProcessedAt == null)
                    .OrderBy(x => x.CreatedAt)
                    .Take(_options.BatchSize)
                    .ToListAsync(stoppingToken);

                if (events.Count > 0)
                {
                    logger.LogInformation("Processing {Count} game events", events.Count);

                    foreach (var gameEvent in events)
                    {
                        await analyticsService.ProcessEvent(gameEvent);
                        gameEvent.ProcessedAt = DateTime.UtcNow;
                    }

                    await db.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing game events");
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.ProcessingIntervalSeconds), stoppingToken);
        }

        logger.LogInformation("GameEventProcessor stopped");
    }
}
