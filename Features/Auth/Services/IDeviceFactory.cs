using Nexus.Features.Auth.Domain;

namespace Nexus.Features.Auth.Services;

public interface IDeviceFactory
{
    DeviceEntity CreateDevice(Guid userId, string deviceId, int expiryDays);
}
