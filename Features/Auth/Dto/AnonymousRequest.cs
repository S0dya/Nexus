using System.ComponentModel.DataAnnotations;

namespace Nexus.Features.Auth.Dto;

public class AnonymousRequest
{
    [Required]
    public string DeviceId { get; set; }
}
