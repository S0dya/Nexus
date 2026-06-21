using System.ComponentModel.DataAnnotations;

namespace Nexus.Features.Auth.Domain;

public class UserEntity
{
    public Guid Id { get; set; }
    [MaxLength(100)]
    public string UserEmail { get; set; }
    [MaxLength(50)]
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public UserRole UserRole { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum UserRole
{
    User,
    Admin,
}