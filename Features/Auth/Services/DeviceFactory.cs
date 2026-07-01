using System.Security.Cryptography;
using Nexus.Features.Auth.Domain;

namespace Nexus.Features.Auth.Services;

public class DeviceFactory : IDeviceFactory
{
    public DeviceEntity CreateDevice(Guid userId, string deviceId, int expiryDays)
    {
        return new DeviceEntity()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            DeviceId = deviceId,
            CreatedAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
        };
    }
}
