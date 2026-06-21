using System.Security.Authentication;
using Microsoft.EntityFrameworkCore;
using Nexus.Database;
using Nexus.Features.Auth.Domain;
using Nexus.Features.Auth.Dto;
using Nexus.Features.Auth.Jwt;
using Nexus.Features.Auth.Validation;

namespace Nexus.Features.Auth.Services;

public class AuthService : IAuthService
{
    private readonly ILogger<AuthService> _logger;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly AppDbContext _db;
    private readonly IPasswordValidation _passwordValidation;

    public AuthService(ILogger<AuthService> logger, 
        IJwtTokenGenerator jwtTokenGenerator,
        AppDbContext db,
        IPasswordValidation passwordValidation)
    {
        _logger = logger;
        _jwtTokenGenerator = jwtTokenGenerator;
        _db = db;
        _passwordValidation = passwordValidation;
    }
    
    public async Task<AuthResponse> Login(LoginRequest request)
    {
        _logger.LogInformation("Login attempt for username {Username}", request.Username);

        _passwordValidation.ValidatePassword(request.Password);
        
        var existingUser = await _db.Users.FirstOrDefaultAsync(user => user.Username == request.Username);
        
        if (existingUser == null)
        {
            _logger.LogWarning("Login failed for username {Username}: user not found", request.Username);
            throw new AuthenticationException("Invalid Credentials");
        }
        if (!BCrypt.Net.BCrypt.Verify(request.Password, existingUser.PasswordHash))
        {
            _logger.LogWarning("Login failed for username {Username}: invalid password", request.Username);
            throw new AuthenticationException("Invalid Credentials");
        }

        _logger.LogInformation("Login succeeded for user {UserId}", existingUser.Id);

        return new ()
        {
            Token = _jwtTokenGenerator.GenerateToken(new JwtUser()
            {
                Id = existingUser.Id,
                Username = existingUser.Username,
                UserRole = existingUser.UserRole,
            }),
        };
    }

    public async Task<AuthResponse> Register(RegisterRequest request)
    {
        _logger.LogInformation("Registration attempt for username {Username}", request.Username);

        _passwordValidation.ValidatePassword(request.Password);
        
        var existingUser = await _db.Users.FirstOrDefaultAsync(user => user.Username == request.Username);

        if (existingUser != null)
        {
            _logger.LogWarning("Registration failed for username {Username}: user already exists", request.Username);
            throw new AuthenticationException("User Already Exists");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var newUser = new UserEntity()
        {
            Id = Guid.NewGuid(),
            UserEmail = "@" + request.Username,
            Username = request.Username,
            PasswordHash = passwordHash,
            UserRole = UserRole.User,
            CreatedAt = DateTime.UtcNow,
        };
        
        _db.Users.Add(newUser);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Registration succeeded for user {UserId}", newUser.Id);
        
        return new ()
        {
            Token = _jwtTokenGenerator.GenerateToken(new JwtUser()
            {
                Id = newUser.Id,
                Username = newUser.Username,
                UserRole = newUser.UserRole,
            }),
        };
    }
}