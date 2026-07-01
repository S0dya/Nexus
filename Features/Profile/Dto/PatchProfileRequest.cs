using System.ComponentModel.DataAnnotations;

namespace Nexus.Features.Profile.Dto;

public class PatchProfileRequest
{
    [MaxLength(100)]
    public string? Name { get; set; }
    public int? IconId { get; set; }
    [MaxLength(500)]
    public string? Bio { get; set; }
    [MaxLength(100)]
    public string? Country { get; set; }
}
