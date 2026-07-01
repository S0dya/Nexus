using System.ComponentModel.DataAnnotations;
using Nexus.Features.Auth.Domain;

namespace Nexus.Features.Registration.Dto;

public class AccountRegistrationRequest
{
    public string DeviceId { get; set; }
    
    [MaxLength(100)]
    public string? UserEmail { get; set; }
    [MaxLength(50)]
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public UserRole UserRole { get; set; }
}