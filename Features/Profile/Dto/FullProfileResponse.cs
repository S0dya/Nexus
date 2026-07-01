namespace Nexus.Features.Profile.Dto;

public class FullProfileResponse
{
    public string Name { get; set; }
    public int? IconId { get; set; }
    public string? Bio { get; set; }
    public string? Country { get; set; }
    public DateTime LastOnline { get; set; }
    public int Level { get; set; }
    public int Experience { get; set; }
    public DateTime CreatedAt { get; set; }
}
