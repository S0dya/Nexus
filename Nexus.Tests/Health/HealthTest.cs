using System.Net;
using Xunit;

namespace Nexus.Tests.Health;

public class HealthTest(WebAppFactory factory) : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _httpClient = factory.CreateClient();

    [Fact]
    public async Task Health_ShouldReturnSuccess()
    {
        var response = await _httpClient.GetAsync("/health");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}