using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Server.Api.IntegrationTests;

public sealed class ApiEndpointsTests
{
    private WebApplicationFactory<Program> factory = null!;
    private HttpClient client = null!;

    [SetUp]
    public void Setup()
    {
        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["Notifications:InboundApiKey"] = "integration-test-notify-key"
                    });
            });
        });
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

    [Test]
    public async Task PostInboundNotification_ShouldReturnUnauthorizedWithoutApiKey()
    {
        var payload = new StringContent("""{"title":"Hello"}""", Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/notifications/inbound", payload);

        Assert.That((int)response.StatusCode, Is.EqualTo(401));
    }

    [Test]
    public async Task PostInboundNotification_ShouldReturnOkWithValidApiKey()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/notifications/inbound")
        {
            Content = new StringContent("""{"title":"Hello","message":"Details"}""", Encoding.UTF8, "application/json")
        };
        request.Headers.Add("X-Notification-Key", "integration-test-notify-key");

        var response = await client.SendAsync(request);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetRecentNotifications_AfterInbound_ShouldReturnTitle()
    {
        var inboundRequest = new HttpRequestMessage(HttpMethod.Post, "/api/notifications/inbound")
        {
            Content = new StringContent("""{"title":"InboundCheck","message":"x"}""", Encoding.UTF8, "application/json")
        };
        inboundRequest.Headers.Add("X-Notification-Key", "integration-test-notify-key");
        var inboundResponse = await client.SendAsync(inboundRequest);
        Assert.That((int)inboundResponse.StatusCode, Is.EqualTo(200));

        var tokenResponse = await client.PostAsync("/api/auth/token?role=Operator", content: null);
        var token = (await tokenResponse.Content.ReadAsStringAsync()).Trim('"');
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var since = Uri.EscapeDataString(DateTimeOffset.MinValue.ToUniversalTime().ToString("o"));
        var getResponse = await client.GetAsync($"/api/notifications/recent?sinceUtc={since}");

        Assert.That((int)getResponse.StatusCode, Is.EqualTo(200));
        var body = await getResponse.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain("InboundCheck"));
    }
}
