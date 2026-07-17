using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Nexus.Features.Profile.Domain;

[Index(nameof(UserId), IsUnique = true)] //do i need to index? 
public class ProfileEntity
{
    public Guid UserId { get; set; }
    [MaxLength(100)]
    public string DisplayName { get; set; }
    public int IconId { get; set; }
    [MaxLength(500)]
    public string? Bio { get; set; }
    [MaxLength(100)]
    public string? Country { get; set; }
    public int Level { get; set; }
    public int Experience { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastOnline { get; set; }
}
