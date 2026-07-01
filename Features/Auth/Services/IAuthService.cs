using Nexus.Features.Auth.Dto;

namespace Nexus.Features.Auth.Services;

public interface IDbAuthService
{
    Task<AuthResponse> Anonymous(AnonymousRequest request);
    Task<AuthResponse> Login(LoginRequest request);
    Task<AuthResponse> Register(RegisterRequest request);
    Task<AuthResponse> LinkAccount(RegisterRequest request);
    Task<AuthResponse> Refresh(RefreshRequest request);
    Task Logout(LogoutRequest request);
}