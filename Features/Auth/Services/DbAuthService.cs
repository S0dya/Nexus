using System.Security.Authentication;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Nexus.Database;
using Nexus.Features.Auth.CurrentUser;
using Nexus.Features.Auth.Domain;
using Nexus.Features.Auth.Dto;
using Nexus.Features.Auth.Jwt;
using Nexus.Features.Auth.Validation;
using Nexus.Features.Registration.Dto;
using Nexus.Features.Registration.Services;
using Nexus.Infrastructure.Security;
using Nexus.Options;
using LoginRequest = Nexus.Features.Auth.Dto.LoginRequest;
using RefreshRequest = Nexus.Features.Auth.Dto.RefreshRequest;
using RegisterRequest = Nexus.Features.Auth.Dto.RegisterRequest;

namespace Nexus.Features.Auth.Services;

public class DbAuthService(
    ILogger<DbAuthService> logger,
    IJwtTokenGenerator jwtTokenGenerator,
    AppDbContext db,
    IOptions<DeviceOptions> deviceOptions,
    IPasswordValidation passwordValidation,
    IPasswordHasher passwordHasher,
    ICurrentUser currentUser,
    IAccountRegistrationService accountRegistrationService,
    IDeviceFactory deviceFactory)
    : IDbAuthService
{
    private readonly DeviceOptions _deviceOptions = deviceOptions.Value;
    
    public async Task<AuthResponse> Anonymous(AnonymousRequest request)
    {
        var result = await accountRegistrationService.CreateAccount(new AccountRegistrationRequest()
        {
            Username = $"guest-{Guid.NewGuid().ToString("N")[..8]}",
            UserEmail = null,
            PasswordHash = "",
            DeviceId = request.DeviceId,
            UserRole = UserRole.Guest
        });
        
        return CreateAuthResponse(result.User, result.Device);
    }

    public async Task<AuthResponse> Login(LoginRequest request)
    {
        logger.LogInformation("Login attempt for username {Username}", request.Username);

        passwordValidation.ValidatePassword(request.Password);
        
        var existingUser = await db.Users.FirstOrDefaultAsync(user => user.Username == request.Username);
        
        if (existingUser == null)
        {
            logger.LogWarning("Login failed for username {Username}: user not found", request.Username);
            throw new AuthenticationException("Invalid Credentials");
        }
        if (!passwordHasher.VerifyPassword(request.Password, existingUser.PasswordHash))
        {
            logger.LogWarning("Login failed for username {Username}: invalid password", request.Username);
            throw new AuthenticationException("Invalid Credentials");
        }
        
        var userDevice = await db.Devices.FirstOrDefaultAsync(device => 
            device.UserId == existingUser.Id 
            && device.DeviceId == request.DeviceId);
        
        if (userDevice == null)
        {
            logger.LogInformation("Creating new device for user {UserId}", existingUser.Id);

            var newDevice = deviceFactory.CreateDevice(existingUser.Id, request.DeviceId, _deviceOptions.ExpiryDays);
            
            db.Devices.Add(newDevice);

            userDevice = newDevice;
        }
        else
        {
            userDevice.ExpiresAt = DateTime.UtcNow.AddDays(_deviceOptions.ExpiryDays);
            userDevice.LastSeenAt = DateTime.UtcNow;
        }
        
        await db.SaveChangesAsync();
        
        logger.LogInformation("Login succeeded for user {UserId}", existingUser.Id);

        return CreateAuthResponse(existingUser, userDevice);
    }

    public async Task<AuthResponse> Register(RegisterRequest request)
    {
        logger.LogInformation("Registration attempt for username {Username}", request.Username);
        
        passwordValidation.ValidatePassword(request.Password);
        
        var existingLoggedUser = await db.Users.FirstOrDefaultAsync(user => user.Username == request.Username);

        if (existingLoggedUser != null)
        {
            logger.LogWarning("Registration failed for username {Username}: user already exists", request.Username);
            throw new AuthenticationException("User Already Exists");
        }

        var result = await accountRegistrationService.CreateAccount(new AccountRegistrationRequest()
        {
            Username = request.Username,
            UserEmail = "@" + request.Username,
            PasswordHash = passwordHasher.HashPassword(request.Password),
            DeviceId = request.DeviceId,
            UserRole = UserRole.User
        });
        
        return CreateAuthResponse(result.User, result.Device);
    }

    public async Task<AuthResponse> LinkAccount(RegisterRequest request)
    {
        logger.LogInformation("LinkAccount attempt for username {Username}", request.Username);

        passwordValidation.ValidatePassword(request.Password);
        
        var existingLoggedUser = await db.Users.FirstOrDefaultAsync(user => user.Username == request.Username);

        if (existingLoggedUser != null)
        {
            logger.LogWarning("LinkAccount failed for username {Username}: user already exists", request.Username);
            throw new AuthenticationException("User Already Exists");
        }

        var existingGuestUser = await db.Users.FirstOrDefaultAsync(user => user.Id == currentUser.UserId);
        
        if (existingGuestUser == null)
        {
            logger.LogWarning("LinkAccount failed for guest as they dont exist");
            throw new AuthenticationException("User Doesn't Exist");
        }
        if (existingGuestUser.UserRole != UserRole.Guest)
        {
            logger.LogWarning("LinkAccount failed for guest as they are not guest");
            throw new AuthenticationException("User Already Registered");
        }
        
        existingGuestUser.UserRole = UserRole.User;
        existingGuestUser.UserEmail = "@" + request.Username;
        existingGuestUser.Username = request.Username;
        existingGuestUser.PasswordHash = passwordHasher.HashPassword(request.Password);

        logger.LogInformation("LinkAccount succeeded for user {UserId}", existingGuestUser.Id);
        
        var existingDevice = await db.Devices.FirstOrDefaultAsync(device => 
            device.UserId == existingGuestUser.Id 
            && device.DeviceId == request.DeviceId);
        
        if (existingDevice == null)
        {
            logger.LogWarning("LinkAccount failed for guest as they dont exist");
            throw new AuthenticationException("Device Doesn't Exist");
        }
        
        existingDevice.ExpiresAt = DateTime.UtcNow.AddDays(_deviceOptions.ExpiryDays);
        existingDevice.LastSeenAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        
        return CreateAuthResponse(existingGuestUser, existingDevice);
    } 
    
    public async Task<AuthResponse> Refresh(RefreshRequest request)
    {
        var existingDevice = await db.Devices.FirstOrDefaultAsync(device => 
            device.RefreshToken == request.RefreshToken 
            && device.DeviceId == request.DeviceId);

        if (existingDevice == null)
        {
            logger.LogWarning("Refresh failed for device is it doesn't exist");
            throw new AuthenticationException("Device Doesn't Exist");
        }
        
        if (existingDevice.ExpiresAt < DateTime.UtcNow)
        {
            logger.LogWarning("Expired refresh token for {UserId}", existingDevice.UserId);
            throw new AuthenticationException("Refresh Token Expired");
        }
        
        existingDevice.LastSeenAt = DateTime.UtcNow;
        existingDevice.ExpiresAt = DateTime.UtcNow.AddDays(_deviceOptions.ExpiryDays);
        await db.SaveChangesAsync();
        
        var existingUser = await db.Users.FirstOrDefaultAsync(user => user.Id == existingDevice.UserId);
        
        if (existingUser == null)
        {
            logger.LogWarning("Refresh failed for guest as they dont exist");
            throw new AuthenticationException("User Doesn't Exist");
        }

        return CreateAuthResponse(existingUser, existingDevice);
    }
    
    public async Task Logout(LogoutRequest request)
    {
        var existingDevice = await db.Devices.FirstOrDefaultAsync(device => 
            device.RefreshToken == request.RefreshToken 
            && device.DeviceId == request.DeviceId);

        if (existingDevice == null)
        {
            logger.LogWarning("Logout failed for device is it doesn't exist");
            throw new AuthenticationException("Device Doesn't Exist");
        }

        existingDevice.ExpiresAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    private AuthResponse CreateAuthResponse(UserEntity user, DeviceEntity device)
    {
        return new AuthResponse()
        {
            AccessToken = jwtTokenGenerator.GenerateToken(new JwtUser()
            {
                Id = user.Id,
                Username = user.Username,
                UserRole = user.UserRole,
            }),
            RefreshToken = device.RefreshToken,
        };
    }
}