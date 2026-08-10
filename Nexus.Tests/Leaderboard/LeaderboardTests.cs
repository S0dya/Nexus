using System.Net;
using System.Net.Http.Json;
using Nexus.Features.Auth.Dto;
using Nexus.Features.Leaderboard.Dto;
using Xunit;

namespace Nexus.Tests.Leaderboard;

public class LeaderboardTests(NexusWebApplicationFactory factory) : IClassFixture<NexusWebApplicationFactory>
{ 
    private readonly HttpClient _httpClient = factory.CreateClient();

    [Fact]
    public async Task SubmitScore_ShouldSubmit()
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

        var submitRequest = new SubmitScoreRequest
        {
            Score = 1000
        };

        var submitResponse = await SubmitScore(authResponse.AccessToken, submitRequest);
        Assert.Equal(HttpStatusCode.OK, submitResponse.StatusCode);
    }

    [Fact]
    public async Task GetGlobalLeaderboard_ShouldReturnLeaderboard()
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

        var submitRequest = new SubmitScoreRequest
        {
            Score = 1000
        };

        var submitResponse = await SubmitScore(authResponse.AccessToken, submitRequest);
        Assert.Equal(HttpStatusCode.OK, submitResponse.StatusCode);

        var leaderboardResponse = await GetGlobalLeaderboard(authResponse.AccessToken, 0, 10);
        Assert.Equal(HttpStatusCode.OK, leaderboardResponse.StatusCode);

        var leaderboard = await leaderboardResponse.Content.ReadFromJsonAsync<GlobalLeaderboardResponse>();
        Assert.NotNull(leaderboard);
        Assert.NotNull(leaderboard.Entries);
    }

    [Fact]
    public async Task GetMyLeaderboard_ShouldReturnMyRank()
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

        var submitRequest = new SubmitScoreRequest
        {
            Score = 1000
        };

        var submitResponse = await SubmitScore(authResponse.AccessToken, submitRequest);
        Assert.Equal(HttpStatusCode.OK, submitResponse.StatusCode);

        var myLeaderboardResponse = await GetMyLeaderboard(authResponse.AccessToken);
        Assert.Equal(HttpStatusCode.OK, myLeaderboardResponse.StatusCode);

        var myLeaderboard = await myLeaderboardResponse.Content.ReadFromJsonAsync<MyLeaderboardResponse>();
        Assert.NotNull(myLeaderboard);
        Assert.NotNull(myLeaderboard.Rank);
        Assert.Equal(1000, myLeaderboard.BestScore);
    }

    [Fact]
    public async Task SubmitScore_WithoutToken_ShouldFail()
    {
        var submitRequest = new SubmitScoreRequest
        {
            Score = 1000
        };

        var submitResponse = await _httpClient.PostAsJsonAsync("/leaderboard/submit", submitRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, submitResponse.StatusCode);
    }

    [Fact]
    public async Task GetGlobalLeaderboard_WithoutToken_ShouldFail()
    {
        var leaderboardResponse = await _httpClient.GetAsync("/leaderboard/global?offset=0&limit=10");
        Assert.Equal(HttpStatusCode.Unauthorized, leaderboardResponse.StatusCode);
    }

    [Fact]
    public async Task GetMyLeaderboard_WithoutToken_ShouldFail()
    {
        var myLeaderboardResponse = await _httpClient.GetAsync("/leaderboard/me");
        Assert.Equal(HttpStatusCode.Unauthorized, myLeaderboardResponse.StatusCode);
    }

    [Fact]
    public async Task SubmitScore_WithNegativeScore_ShouldSucceed()
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

        var submitRequest = new SubmitScoreRequest
        {
            Score = -100
        };

        var submitResponse = await SubmitScore(authResponse.AccessToken, submitRequest);
        Assert.Equal(HttpStatusCode.OK, submitResponse.StatusCode);
    }

    [Fact]
    public async Task SubmitScore_ShouldUpdateBestScore()
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

        var submitRequest1 = new SubmitScoreRequest
        {
            Score = 1000
        };

        var submitResponse1 = await SubmitScore(authResponse.AccessToken, submitRequest1);
        Assert.Equal(HttpStatusCode.OK, submitResponse1.StatusCode);

        var myLeaderboardResponse1 = await GetMyLeaderboard(authResponse.AccessToken);
        var myLeaderboard1 = await myLeaderboardResponse1.Content.ReadFromJsonAsync<MyLeaderboardResponse>();
        Assert.Equal(1000, myLeaderboard1.BestScore);

        var submitRequest2 = new SubmitScoreRequest
        {
            Score = 2000
        };

        var submitResponse2 = await SubmitScore(authResponse.AccessToken, submitRequest2);
        Assert.Equal(HttpStatusCode.OK, submitResponse2.StatusCode);

        var myLeaderboardResponse2 = await GetMyLeaderboard(authResponse.AccessToken);
        var myLeaderboard2 = await myLeaderboardResponse2.Content.ReadFromJsonAsync<MyLeaderboardResponse>();
        Assert.Equal(2000, myLeaderboard2.BestScore);
    }

    [Fact]
    public async Task SubmitScore_WithLowerScore_ShouldNotUpdateBestScore()
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

        var submitRequest1 = new SubmitScoreRequest
        {
            Score = 2000
        };

        var submitResponse1 = await SubmitScore(authResponse.AccessToken, submitRequest1);
        Assert.Equal(HttpStatusCode.OK, submitResponse1.StatusCode);

        var myLeaderboardResponse1 = await GetMyLeaderboard(authResponse.AccessToken);
        var myLeaderboard1 = await myLeaderboardResponse1.Content.ReadFromJsonAsync<MyLeaderboardResponse>();
        Assert.Equal(2000, myLeaderboard1.BestScore);

        var submitRequest2 = new SubmitScoreRequest
        {
            Score = 1000
        };

        var submitResponse2 = await SubmitScore(authResponse.AccessToken, submitRequest2);
        Assert.Equal(HttpStatusCode.OK, submitResponse2.StatusCode);

        var myLeaderboardResponse2 = await GetMyLeaderboard(authResponse.AccessToken);
        var myLeaderboard2 = await myLeaderboardResponse2.Content.ReadFromJsonAsync<MyLeaderboardResponse>();
        Assert.Equal(2000, myLeaderboard2.BestScore);
    }

    [Fact]
    public async Task GetGlobalLeaderboard_WithPagination_ShouldReturnCorrectCount()
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

        var submitRequest = new SubmitScoreRequest
        {
            Score = 1000
        };

        var submitResponse = await SubmitScore(authResponse.AccessToken, submitRequest);
        Assert.Equal(HttpStatusCode.OK, submitResponse.StatusCode);

        var leaderboardResponse = await GetGlobalLeaderboard(authResponse.AccessToken, 0, 5);
        Assert.Equal(HttpStatusCode.OK, leaderboardResponse.StatusCode);

        var leaderboard = await leaderboardResponse.Content.ReadFromJsonAsync<GlobalLeaderboardResponse>();
        Assert.NotNull(leaderboard);
        Assert.NotNull(leaderboard.Entries);
        Assert.True(leaderboard.Entries.Count <= 5);
    }

    [Fact]
    public async Task GetMyLeaderboard_WhenNoScoreSubmitted_ShouldReturnZero()
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

        var myLeaderboardResponse = await GetMyLeaderboard(authResponse.AccessToken);
        Assert.Equal(HttpStatusCode.OK, myLeaderboardResponse.StatusCode);

        var myLeaderboard = await myLeaderboardResponse.Content.ReadFromJsonAsync<MyLeaderboardResponse>();
        Assert.NotNull(myLeaderboard);
        Assert.Equal(0, myLeaderboard.BestScore);
        Assert.Null(myLeaderboard.Rank);
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

    private async Task<HttpResponseMessage> SubmitScore(string accessToken, SubmitScoreRequest request)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/leaderboard/submit");
        httpRequest.Headers.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        httpRequest.Content = JsonContent.Create(request);

        return await _httpClient.SendAsync(httpRequest);
    }

    private async Task<HttpResponseMessage> GetGlobalLeaderboard(string accessToken, int offset, int limit)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/leaderboard/global?offset={offset}&limit={limit}");
        request.Headers.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        return await _httpClient.SendAsync(request);
    }

    private async Task<HttpResponseMessage> GetMyLeaderboard(string accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/leaderboard/me");
        request.Headers.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        return await _httpClient.SendAsync(request);
    }
}
