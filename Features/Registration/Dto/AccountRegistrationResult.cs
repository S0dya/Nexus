using System.ComponentModel.DataAnnotations;
using Nexus.Features.Auth.Domain;

namespace Nexus.Features.Registration.Dto;

public class AccountRegistrationResult
{
    public UserEntity User { get; set; }
    public DeviceEntity Device { get; set; }
}