using Nexus.Features.GameEvent.Domain;

namespace Nexus.Features.Analytics.Services;

public interface IAnalyticsService
{
    Task ProcessEvent(GameEventEntity gameEvent);
}