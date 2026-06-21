namespace Nexus.Features.Auth.Jwt;

public interface IJwtTokenGenerator
{
    string GenerateToken(JwtUser user);
}