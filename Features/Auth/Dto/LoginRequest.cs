using System.ComponentModel.DataAnnotations;

namespace Nexus.Features.Auth.Dto;

public class LoginRequest
{
    [Required]
    // [EmailAddress]
    // public string Email { get; set; }
    [MaxLength(50)]
    [MinLength(3)]
    public string Username { get; set; }
    [Required]
    public string Password { get; set; }
}