using Nexus.Features.Auth.Domain;

namespace Nexus.Features.Auth.Jwt;

public class JwtUser
{
    public Guid Id { get; set; }
    // public string Password { get; set; }
    public string Username { get; set; }
    public UserRole UserRole { get; set; }
}