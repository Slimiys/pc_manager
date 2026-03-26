using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Server.Api.IntegrationTests;

public sealed class ApiEndpointsTests
{
    private WebApplicationFactory<Program> factory = null!;
    private HttpClient client = null!;

    [SetUp]
    public void Setup()
    {
        factory = new WebApplicationFactory<Program>();
        client = factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        client.Dispose();
        factory.Dispose();
    }

    [Test]
    public async Task GetSystemInfo_ShouldReturnUnauthorizedWithoutToken()
    {
        var response = await client.GetAsync("/api/system/info");
        Assert.That((int)response.StatusCode, Is.EqualTo(401));
    }

    [Test]
    public async Task GetSystemInfo_ShouldReturnOkWithToken()
    {
        var tokenResponse = await client.PostAsync("/api/auth/token?role=Operator", content: null);
        var token = await tokenResponse.Content.ReadAsStringAsync();
        token = token.Trim('"');

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.GetAsync("/api/system/info");

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task ExecuteCommand_ShouldReturnValidationForUnknownType()
    {
        var tokenResponse = await client.PostAsync("/api/auth/token?role=Operator", content: null);
        var token = (await tokenResponse.Content.ReadAsStringAsync()).Trim('"');
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new StringContent("""{"type":"Unknown","payload":null}""", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/commands/execute", payload);

        Assert.That((int)response.StatusCode, Is.EqualTo(400));
    }
}
