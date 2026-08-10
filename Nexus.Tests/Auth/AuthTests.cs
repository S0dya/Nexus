using System.Net;
using System.Net.Http.Json;
using Nexus.Features.Auth.Dto;
using Xunit;

namespace Nexus.Tests.Auth;

public class AuthTests(NexusWebApplicationFactory factory) : IClassFixture<NexusWebApplicationFactory>
{ 
    private readonly HttpClient _httpClient = factory.CreateClient();


    [Fact]
    public async Task Anonymous_ShouldCreateAccount()
    {
        var request = new AnonymousRequest()
        {
            DeviceId = Guid.NewGuid().ToString(),
        };

        var response = await _httpClient.PostAsJsonAsync("/auth/anonymous", request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        
        Assert.NotNull(authResponse);
        Assert.False(String.IsNullOrWhiteSpace(authResponse.AccessToken));
        Assert.False(String.IsNullOrWhiteSpace(authResponse.RefreshToken));
    }

    [Fact]
    public async Task Register_ShouldCreateAccount()
    {
        var username = $"test_{Guid.NewGuid()}";
        var password = "Asd123)";
        var deviceId = Guid.NewGuid().ToString();

        var anonResponse = await AnonymousUser(deviceId);
        Assert.Equal(HttpStatusCode.OK, anonResponse.StatusCode);

        var response = await RegisterUser(username, password, deviceId);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(authResponse);
        Assert.False(string.IsNullOrWhiteSpace(authResponse.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(authResponse.RefreshToken));
    }
    
    [Fact]
    public async Task Login_WithWrongPassword_ShouldFail()
    {
        var username = $"test_{Guid.NewGuid()}";        
        var password = "Asd123)";
        var deviceId = Guid.NewGuid().ToString();

        var anonResponse = await AnonymousUser(deviceId);
        Assert.Equal(HttpStatusCode.OK, anonResponse.StatusCode);

        var registerResponse = await RegisterUser(username, password, deviceId);

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var loginResponse = await LoginUser(username, "Wrong123)", deviceId);

        Assert.Equal(HttpStatusCode.InternalServerError, loginResponse.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldLoginExistingUser()
    {
        var username = $"test_{Guid.NewGuid()}";
        var password = "Asd123)";
        var deviceId = Guid.NewGuid().ToString();

        var anonResponse = await AnonymousUser(deviceId);
        Assert.Equal(HttpStatusCode.OK, anonResponse.StatusCode);

        var registerResponse = await RegisterUser(username, password, deviceId);

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var loginResponse = await LoginUser(username, password, deviceId);

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(authResponse);
        Assert.False(string.IsNullOrWhiteSpace(authResponse.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(authResponse.RefreshToken));
    }

    [Fact]
    public async Task Refresh_ShouldReturnNewTokens()
    {
        var username = $"test_{Guid.NewGuid()}";
        var password = "Asd123)";
        var deviceId = Guid.NewGuid().ToString();

        var anonResponse = await AnonymousUser(deviceId);
        Assert.Equal(HttpStatusCode.OK, anonResponse.StatusCode);

        var registerResponse = await RegisterUser(username, password, deviceId);

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(authResponse);

        var refreshResponse = await RefreshToken(authResponse.AccessToken, authResponse.RefreshToken, deviceId);

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        var newAuthResponse = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(newAuthResponse);
        Assert.False(string.IsNullOrWhiteSpace(newAuthResponse.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(newAuthResponse.RefreshToken));
    }

    [Fact]
    public async Task Logout_ShouldSucceed()
    {
        var username = $"test_{Guid.NewGuid()}";
        var password = "Asd123)";
        var deviceId = Guid.NewGuid().ToString();

        var anonResponse = await AnonymousUser(deviceId);
        Assert.Equal(HttpStatusCode.OK, anonResponse.StatusCode);

        var registerResponse = await RegisterUser(username, password, deviceId);

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(authResponse);

        var logoutResponse = await LogoutUser(authResponse.AccessToken, authResponse.RefreshToken, deviceId);

        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);
    }

    [Fact]
    public async Task Me_ShouldReturnCurrentUser()
    {
        var username = $"test_{Guid.NewGuid()}";
        var password = "Asd123)";
        var deviceId = Guid.NewGuid().ToString();

        var anonResponse = await AnonymousUser(deviceId);
        Assert.Equal(HttpStatusCode.OK, anonResponse.StatusCode);

        var registerResponse = await RegisterUser(username, password, deviceId);

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(authResponse);

        var meResponse = await GetMe(authResponse.AccessToken);

        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var userResponse = await meResponse.Content.ReadFromJsonAsync<UserResponse>();

        Assert.NotNull(userResponse);
        Assert.Equal(username, userResponse.Username);
    }



    private async Task<HttpResponseMessage> AnonymousUser(string deviceId)
    {
        var request = new AnonymousRequest
        {
            DeviceId = deviceId,
        };

        return await _httpClient.PostAsJsonAsync("/auth/anonymous", request);
    }

    private async Task<HttpResponseMessage> RegisterUser(string username, string password, string deviceId)
    {
        var request = new RegisterRequest
        {
            Username = username,
            Password = password,
            DeviceId = deviceId,
        };

        return await _httpClient.PostAsJsonAsync("/auth/register", request);
    }

    private async Task<HttpResponseMessage> LoginUser(string username, string password, string deviceId)
    {
        var request = new LoginRequest
        {
            Username = username,
            Password = password,
            DeviceId = deviceId,
        };

        return await _httpClient.PostAsJsonAsync("/auth/login", request);
    }

    private async Task<HttpResponseMessage> RefreshToken(string accessToken, string refreshToken, string deviceId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/refresh");
        request.Headers.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        
        var body = new RefreshRequest
        {
            RefreshToken = refreshToken,
            DeviceId = deviceId,
        };
        
        request.Content = JsonContent.Create(body);

        return await _httpClient.SendAsync(request);
    }

    private async Task<HttpResponseMessage> LogoutUser(string accessToken, string refreshToken, string deviceId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/logout");
        request.Headers.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        
        var body = new LogoutRequest
        {
            RefreshToken = refreshToken,
            DeviceId = deviceId,
        };
        
        request.Content = JsonContent.Create(body);

        return await _httpClient.SendAsync(request);
    }

    private async Task<HttpResponseMessage> GetMe(string accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/auth/me");
        request.Headers.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        return await _httpClient.SendAsync(request);
    }

    [Fact]
    public async Task Me_WithoutToken_ShouldFail()
    {
        var meResponse = await _httpClient.GetAsync("/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, meResponse.StatusCode);
    }

    [Fact]
    public async Task Logout_WithoutToken_ShouldFail()
    {
        var logoutResponse = await _httpClient.PostAsync("/auth/logout", null);

        Assert.Equal(HttpStatusCode.Unauthorized, logoutResponse.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_ShouldFail()
    {
        var deviceId = Guid.NewGuid().ToString();
        var refreshResponse = await RefreshToken("invalid_access_token", "invalid_refresh_token", deviceId);

        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ShouldFail()
    {
        var username = $"test_{Guid.NewGuid()}";
        var password = "Asd123)";
        var deviceId = Guid.NewGuid().ToString();

        var anonResponse = await AnonymousUser(deviceId);
        Assert.Equal(HttpStatusCode.OK, anonResponse.StatusCode);

        var registerResponse = await RegisterUser(username, password, deviceId);

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var duplicateResponse = await RegisterUser(username, password, deviceId);

        Assert.Equal(HttpStatusCode.InternalServerError, duplicateResponse.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnknownUser_ShouldFail()
    {
        var username = $"test_{Guid.NewGuid()}";
        var password = "Asd123)";
        var deviceId = Guid.NewGuid().ToString();

        var loginResponse = await LoginUser(username, password, deviceId);

        Assert.Equal(HttpStatusCode.InternalServerError, loginResponse.StatusCode);
    }


}