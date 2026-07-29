using Microsoft.EntityFrameworkCore;
using Nexus.Features.GameEvent.Dto;

namespace Nexus.Features.GameEvent.Domain;

[Index(nameof(ProcessedAt), nameof(CreatedAt))]
public class GameEventEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public GameEventType Type { get; set; }
    public string Payload { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}