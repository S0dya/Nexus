using System.ComponentModel.DataAnnotations;

namespace Nexus.Features.Auth.Dto;

public class LogoutRequest
{
    [Required]
    [MinLength(3)]
    public string RefreshToken { get; set; }
    [Required]
    public string DeviceId { get; set; }
}
