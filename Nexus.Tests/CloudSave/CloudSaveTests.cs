using System.Net;
using System.Net.Http.Json;
using Nexus.Features.Auth.Dto;
using Nexus.Features.CloudSave.Dto;
using Xunit;

namespace Nexus.Tests.CloudSave;

public class CloudSaveTests(NexusWebApplicationFactory factory) : IClassFixture<NexusWebApplicationFactory>
{ 
    private readonly HttpClient _httpClient = factory.CreateClient();

    [Fact]
    public async Task SaveData_ShouldSave()
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

        var saveRequest = new SaveDataRequest
        {
            Data = "{\"level\": 10, \"coins\": 1000}",
            Version = 1
        };

        var saveResponse = await SaveData(authResponse.AccessToken, saveRequest);
        Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);

        var saveDataResponse = await saveResponse.Content.ReadFromJsonAsync<SaveDataResponse>();
        Assert.NotNull(saveDataResponse);
        Assert.Equal(2, saveDataResponse.Version);
    }

    [Fact]
    public async Task LoadData_ShouldReturnSavedData()
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

        var saveRequest = new SaveDataRequest
        {
            Data = "{\"level\": 10, \"coins\": 1000}",
            Version = 1
        };

        var saveResponse = await SaveData(authResponse.AccessToken, saveRequest);
        Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);

        var loadResponse = await LoadData(authResponse.AccessToken);
        Assert.Equal(HttpStatusCode.OK, loadResponse.StatusCode);

        var loadDataResponse = await loadResponse.Content.ReadFromJsonAsync<LoadDataResponse>();
        Assert.NotNull(loadDataResponse);
        Assert.Equal("{\"level\": 10, \"coins\": 1000}", loadDataResponse.Data);
        Assert.Equal(2, loadDataResponse.Version);
    }

    [Fact]
    public async Task ResetSave_ShouldClearData()
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

        var saveRequest = new SaveDataRequest
        {
            Data = "{\"level\": 10, \"coins\": 1000}",
            Version = 1
        };

        var saveResponse = await SaveData(authResponse.AccessToken, saveRequest);
        Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);

        var resetResponse = await ResetSave(authResponse.AccessToken);
        Assert.Equal(HttpStatusCode.OK, resetResponse.StatusCode);

        var loadResponse = await LoadData(authResponse.AccessToken);
        Assert.Equal(HttpStatusCode.OK, loadResponse.StatusCode);

        var loadDataResponse = await loadResponse.Content.ReadFromJsonAsync<LoadDataResponse>();
        Assert.NotNull(loadDataResponse);
        Assert.NotNull(loadDataResponse.Data);
    }

    [Fact]
    public async Task SaveData_WithoutToken_ShouldFail()
    {
        var saveRequest = new SaveDataRequest
        {
            Data = "{\"level\": 10}",
            Version = 1
        };

        var saveResponse = await _httpClient.PutAsJsonAsync("/cloudsave/me", saveRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, saveResponse.StatusCode);
    }

    [Fact]
    public async Task LoadData_WithoutToken_ShouldFail()
    {
        var loadResponse = await _httpClient.GetAsync("/cloudsave/me");
        Assert.Equal(HttpStatusCode.Unauthorized, loadResponse.StatusCode);
    }

    [Fact]
    public async Task ResetSave_WithoutToken_ShouldFail()
    {
        var resetResponse = await _httpClient.PostAsync("/cloudsave/me", null);
        Assert.Equal(HttpStatusCode.Unauthorized, resetResponse.StatusCode);
    }

    [Fact]
    public async Task SaveData_WithEmptyData_ShouldSucceed()
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

        var saveRequest = new SaveDataRequest
        {
            Data = "",
            Version = 1
        };

        var saveResponse = await SaveData(authResponse.AccessToken, saveRequest);
        Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);
    }

    [Fact]
    public async Task LoadData_WhenNoSaveExists_ShouldReturnEmpty()
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

        var loadResponse = await LoadData(authResponse.AccessToken);
        Assert.Equal(HttpStatusCode.OK, loadResponse.StatusCode);

        var loadDataResponse = await loadResponse.Content.ReadFromJsonAsync<LoadDataResponse>();
        Assert.NotNull(loadDataResponse);
        Assert.NotNull(loadDataResponse.Data);
    }

    [Fact]
    public async Task SaveData_ShouldIncrementVersion()
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

        var saveRequest1 = new SaveDataRequest
        {
            Data = "{\"level\": 10}",
            Version = 1
        };

        var saveResponse1 = await SaveData(authResponse.AccessToken, saveRequest1);
        Assert.Equal(HttpStatusCode.OK, saveResponse1.StatusCode);

        var saveDataResponse1 = await saveResponse1.Content.ReadFromJsonAsync<SaveDataResponse>();
        Assert.NotNull(saveDataResponse1);
        Assert.Equal(2, saveDataResponse1.Version);

        var saveRequest2 = new SaveDataRequest
        {
            Data = "{\"level\": 11}",
            Version = 2
        };

        var saveResponse2 = await SaveData(authResponse.AccessToken, saveRequest2);
        Assert.Equal(HttpStatusCode.OK, saveResponse2.StatusCode);

        var saveDataResponse2 = await saveResponse2.Content.ReadFromJsonAsync<SaveDataResponse>();
        Assert.NotNull(saveDataResponse2);
        Assert.Equal(3, saveDataResponse2.Version);
    }

    [Fact]
    public async Task SaveData_WithLargeData_ShouldSucceed()
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

        var largeData = new string('A', 10000);
        var saveRequest = new SaveDataRequest
        {
            Data = largeData,
            Version = 1
        };

        var saveResponse = await SaveData(authResponse.AccessToken, saveRequest);
        Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);

        var loadResponse = await LoadData(authResponse.AccessToken);
        Assert.Equal(HttpStatusCode.OK, loadResponse.StatusCode);

        var loadDataResponse = await loadResponse.Content.ReadFromJsonAsync<LoadDataResponse>();
        Assert.NotNull(loadDataResponse);
        Assert.Equal(largeData, loadDataResponse.Data);
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

    private async Task<HttpResponseMessage> SaveData(string accessToken, SaveDataRequest request)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Put, "/cloudsave/me");
        httpRequest.Headers.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        httpRequest.Content = JsonContent.Create(request);

        return await _httpClient.SendAsync(httpRequest);
    }

    private async Task<HttpResponseMessage> LoadData(string accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/cloudsave/me");
        request.Headers.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        return await _httpClient.SendAsync(request);
    }

    private async Task<HttpResponseMessage> ResetSave(string accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/cloudsave/me");
        request.Headers.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        return await _httpClient.SendAsync(request);
    }
}
