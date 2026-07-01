namespace Nexus.Features.Profile.Dto;

public class ProfileResponse
{
    public string Name { get; set; }
    public int? IconId { get; set; }
    public string? Bio { get; set; }
    public string? Country { get; set; }
    public DateTime LastOnline { get; set; }
    public int Level { get; set; }
}
