using Nexus.Features.GameEvent.Dto;
using Nexus.Features.Shop.Dto;

namespace Nexus.Features.GameEvent.Services;

public interface IGameEventService
{
    Task AddEvent<TPayload>(GameEventType type, TPayload payload);
}