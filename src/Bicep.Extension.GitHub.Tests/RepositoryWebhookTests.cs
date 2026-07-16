using System.Net;
using System.Text.Json;
using Bicep.Extension.Github.Handlers;

namespace Bicep.Extension.Github.Tests;

[TestClass]
public sealed class RepositoryWebhookTests
{
    private const string HookJson =
        """
        {
            "id": 1,
            "name": "web",
            "active": true,
            "events": ["push"],
            "config": {
                "url": "https://example.com/hook",
                "content_type": "json"
            },
            "url": "https://api.github.com/repos/acme/widgets/hooks/1"
        }
        """;

    [TestMethod]
    public async Task Creates_webhook_when_absent()
    {
        var mock = new MockHttpMessageHandler((request, _) =>
            request.Method == HttpMethod.Get
                ? MockHttpMessageHandler.Json(HttpStatusCode.OK, "[]")
                : MockHttpMessageHandler.Json(HttpStatusCode.Created, HookJson));

        var handler = new RepositoryWebhookHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "RepositoryWebhook", new
        {
            owner = "acme",
            repo = "widgets",
            url = "https://example.com/hook",
            events = new[] { "push", "pull_request" },
            active = true,
            contentType = "json",
        });

        Assert.IsNull(response.ErrorData);

        var create = mock.Requests.Single(r => r.Method == HttpMethod.Post);
        Assert.AreEqual("/repos/acme/widgets/hooks", create.Uri.AbsolutePath);

        var body = JsonSerializer.Deserialize<JsonElement>(create.Body);
        Assert.AreEqual("web", body.GetProperty("name").GetString());
        Assert.AreEqual("https://example.com/hook", body.GetProperty("config").GetProperty("url").GetString());
    }

    [TestMethod]
    public async Task Does_not_return_secret_to_bicep()
    {
        var mock = new MockHttpMessageHandler((request, _) =>
            request.Method == HttpMethod.Get
                ? MockHttpMessageHandler.Json(HttpStatusCode.OK, "[]")
                : MockHttpMessageHandler.Json(HttpStatusCode.Created, HookJson));

        var handler = new RepositoryWebhookHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "RepositoryWebhook", new
        {
            owner = "acme",
            repo = "widgets",
            url = "https://example.com/hook",
            secret = "shhh",
            active = true,
            contentType = "json",
        });

        var properties = response.ResourceProperties();
        Assert.IsFalse(
            properties.TryGetProperty("secret", out var secret) && secret.ValueKind != JsonValueKind.Null,
            "the webhook secret must not be echoed back to Bicep");
    }
}
