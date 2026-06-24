using Microsoft.EntityFrameworkCore;

namespace Nexus.Features.Auth.Domain;

[Index(nameof(UserId), nameof(DeviceId), IsUnique = true)]
public class DeviceEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string RefreshToken { get; set; }
    public string DeviceId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastSeenAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}