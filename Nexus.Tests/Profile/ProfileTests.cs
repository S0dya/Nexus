using System.Net;
using System.Net.Http.Json;
using Nexus.Features.Auth.Dto;
using Nexus.Features.Profile.Dto;
using Xunit;

namespace Nexus.Tests.Profile;

public class ProfileTests(NexusWebApplicationFactory factory) : IClassFixture<NexusWebApplicationFactory>
{ 
    private readonly HttpClient _httpClient = factory.CreateClient();

    [Fact]
    public async Task GetUser_ShouldReturnProfile()
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

        var meResponse = await GetAuthMe(authResponse.AccessToken);
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var userResponse = await meResponse.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(userResponse);

        var profileResponse = await GetUser(userResponse.UserId);
        Assert.Equal(HttpStatusCode.OK, profileResponse.StatusCode);

        var profile = await profileResponse.Content.ReadFromJsonAsync<ProfileResponse>();
        Assert.NotNull(profile);
    }

    [Fact]
    public async Task GetUser_WithInvalidId_ShouldFail()
    {
        var invalidUserId = Guid.NewGuid();
        var profileResponse = await GetUser(invalidUserId);
        
        Assert.Equal(HttpStatusCode.NotFound, profileResponse.StatusCode);
    }

    [Fact]
    public async Task GetMe_ShouldReturnCurrentUserProfile()
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

        var profileResponse = await GetProfileMe(authResponse.AccessToken);
        Assert.Equal(HttpStatusCode.OK, profileResponse.StatusCode);

        var profile = await profileResponse.Content.ReadFromJsonAsync<FullProfileResponse>();
        Assert.NotNull(profile);
    }

    [Fact]
    public async Task GetMe_WithoutToken_ShouldFail()
    {
        var profileResponse = await _httpClient.GetAsync("/profile/me");
        Assert.Equal(HttpStatusCode.Unauthorized, profileResponse.StatusCode);
    }

    [Fact]
    public async Task PatchMe_ShouldUpdateProfile()
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

        var patchRequest = new PatchProfileRequest
        {
            Name = "Updated Name",
            Bio = "Updated bio",
            Country = "US",
            IconId = 5
        };

        var patchResponse = await PatchProfileMe(authResponse.AccessToken, patchRequest);
        Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);

        var getResponse = await GetProfileMe(authResponse.AccessToken);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var profile = await getResponse.Content.ReadFromJsonAsync<FullProfileResponse>();
        Assert.NotNull(profile);
        Assert.Equal("Updated Name", profile.Name);
        Assert.Equal("Updated bio", profile.Bio);
        Assert.Equal("US", profile.Country);
        Assert.Equal(5, profile.IconId);
    }

    [Fact]
    public async Task PatchMe_WithoutToken_ShouldFail()
    {
        var patchRequest = new PatchProfileRequest
        {
            Name = "Test Name"
        };

        var patchResponse = await _httpClient.PatchAsJsonAsync("/profile/me", patchRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, patchResponse.StatusCode);
    }

    [Fact]
    public async Task PatchMe_WithInvalidData_ShouldFail()
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

        var patchRequest = new PatchProfileRequest
        {
            Name = new string('A', 101) // Exceeds MaxLength(100)
        };

        var patchResponse = await PatchProfileMe(authResponse.AccessToken, patchRequest);
        Assert.Equal(HttpStatusCode.BadRequest, patchResponse.StatusCode);
    }

    [Fact]
    public async Task PatchMe_WithPartialUpdate_ShouldSucceed()
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

        var patchRequest = new PatchProfileRequest
        {
            Bio = "Just updating bio"
        };

        var patchResponse = await PatchProfileMe(authResponse.AccessToken, patchRequest);
        Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);
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

    private async Task<HttpResponseMessage> GetUser(Guid userId)
    {
        return await _httpClient.GetAsync($"/profile/user/{userId}");
    }

    private async Task<HttpResponseMessage> GetAuthMe(string accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/auth/me");
        request.Headers.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        return await _httpClient.SendAsync(request);
    }

    private async Task<HttpResponseMessage> GetProfileMe(string accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/profile/me");
        request.Headers.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        return await _httpClient.SendAsync(request);
    }

    private async Task<HttpResponseMessage> PatchProfileMe(string accessToken, PatchProfileRequest request)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Patch, "/profile/me");
        httpRequest.Headers.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        httpRequest.Content = JsonContent.Create(request);

        return await _httpClient.SendAsync(httpRequest);
    }
}
