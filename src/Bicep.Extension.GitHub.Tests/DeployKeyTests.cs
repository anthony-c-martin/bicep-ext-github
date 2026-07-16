using System.Net;
using System.Text.Json;
using Bicep.Extension.Github.Handlers;

namespace Bicep.Extension.Github.Tests;

[TestClass]
public sealed class DeployKeyTests
{
    private const string CreatedKeyJson =
        """
        {
            "id": 1,
            "key": "ssh-ed25519 AAAAC3Nz",
            "title": "ci-key",
            "read_only": true,
            "verified": true,
            "url": "https://api.github.com/repos/acme/widgets/keys/1",
            "created_at": "2020-01-01T00:00:00Z"
        }
        """;

    [TestMethod]
    public async Task Creates_deploy_key_when_absent()
    {
        var mock = new MockHttpMessageHandler((request, _) =>
            request.Method == HttpMethod.Get
                ? MockHttpMessageHandler.Json(HttpStatusCode.OK, "[]")
                : MockHttpMessageHandler.Json(HttpStatusCode.Created, CreatedKeyJson));

        var handler = new DeployKeyHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "DeployKey", new
        {
            owner = "acme",
            repo = "widgets",
            title = "ci-key",
            key = "ssh-ed25519 AAAAC3Nz",
            readOnly = true,
        });

        Assert.IsNull(response.ErrorData);

        var create = mock.Requests.Single(r => r.Method == HttpMethod.Post);
        Assert.AreEqual("/repos/acme/widgets/keys", create.Uri.AbsolutePath);

        var body = JsonSerializer.Deserialize<JsonElement>(create.Body);
        Assert.AreEqual("ci-key", body.GetProperty("title").GetString());
        Assert.AreEqual("ssh-ed25519 AAAAC3Nz", body.GetProperty("key").GetString());
        Assert.IsTrue(body.GetProperty("read_only").GetBoolean());
    }

    [TestMethod]
    public async Task Does_not_recreate_when_key_unchanged()
    {
        var existing =
            """
            [
                {
                    "id": 1,
                    "key": "ssh-ed25519 AAAAC3Nz",
                    "title": "ci-key",
                    "read_only": true,
                    "verified": true,
                    "url": "https://api.github.com/repos/acme/widgets/keys/1"
                }
            ]
            """;

        var mock = new MockHttpMessageHandler((request, _) =>
            request.Method == HttpMethod.Get
                ? MockHttpMessageHandler.Json(HttpStatusCode.OK, existing)
                : MockHttpMessageHandler.Json(HttpStatusCode.Created, CreatedKeyJson));

        var handler = new DeployKeyHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "DeployKey", new
        {
            owner = "acme",
            repo = "widgets",
            title = "ci-key",
            key = "ssh-ed25519 AAAAC3Nz",
            readOnly = true,
        });

        Assert.IsNull(response.ErrorData);
        Assert.IsFalse(mock.Requests.Any(r => r.Method == HttpMethod.Post), "unchanged key should not be recreated");
        Assert.IsFalse(mock.Requests.Any(r => r.Method == HttpMethod.Delete));
    }
}
