using System.Text.Json;
using Nexus.Database;
using Nexus.Features.Auth.CurrentUser;
using Nexus.Features.GameEvent.Domain;
using Nexus.Features.GameEvent.Dto;

namespace Nexus.Features.GameEvent.Services;

public class DbGameEventService(ILogger<DbGameEventService> logger,
    AppDbContext db,
    ICurrentUser currentUser) : IGameEventService
{
    public async Task AddEvent<TPayload>(GameEventType type, TPayload payload)
    {
        var serializedPayload = JsonSerializer.Serialize(payload);
        
        logger.LogDebug("Adding {Type} event for {UserId}", type, currentUser.UserId);
        
        db.GameEvents.Add(new GameEventEntity
        {
            Id = Guid.NewGuid(),
            UserId = currentUser.UserId,
            Type = type,
            Payload = serializedPayload,
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = null,
        });
    }
}