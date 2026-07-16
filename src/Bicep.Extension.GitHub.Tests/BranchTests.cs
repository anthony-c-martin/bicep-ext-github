using System.Net;
using System.Text.Json;
using Bicep.Extension.Github.Handlers;

namespace Bicep.Extension.Github.Tests;

[TestClass]
public sealed class BranchTests
{
    private const string NotFoundJson =
        """
        {
            "message": "Not Found"
        }
        """;

    private const string MainRef =
        """
        {
            "ref": "refs/heads/main",
            "node_id": "n",
            "url": "https://api.github.com/repos/acme/widgets/git/refs/heads/main",
            "object": {
                "type": "commit",
                "sha": "abc123",
                "url": "https://api.github.com/repos/acme/widgets/git/commits/abc123"
            }
        }
        """;

    [TestMethod]
    public async Task Creates_branch_from_source_branch()
    {
        var newRef =
            """
            {
                "ref": "refs/heads/feature",
                "node_id": "n",
                "url": "https://api.github.com/repos/acme/widgets/git/refs/heads/feature",
                "object": {
                    "type": "commit",
                    "sha": "abc123",
                    "url": "https://api.github.com/repos/acme/widgets/git/commits/abc123"
                }
            }
            """;

        var mock = new MockHttpMessageHandler((request, _) =>
        {
            if (request.Method == HttpMethod.Get)
            {
                return request.RequestUri!.AbsolutePath.EndsWith("/heads/feature", StringComparison.Ordinal)
                    ? MockHttpMessageHandler.Json(HttpStatusCode.NotFound, NotFoundJson)
                    : MockHttpMessageHandler.Json(HttpStatusCode.OK, MainRef);
            }

            return MockHttpMessageHandler.Json(HttpStatusCode.Created, newRef);
        });

        var handler = new BranchHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "Branch", new
        {
            owner = "acme",
            repo = "widgets",
            name = "feature",
            sourceBranch = "main",
        });

        Assert.IsNull(response.ErrorData);

        var create = mock.Requests.Single(r => r.Method == HttpMethod.Post);
        Assert.AreEqual("/repos/acme/widgets/git/refs", create.Uri.AbsolutePath);

        var body = JsonSerializer.Deserialize<JsonElement>(create.Body);
        Assert.AreEqual("refs/heads/feature", body.GetProperty("ref").GetString());
        Assert.AreEqual("abc123", body.GetProperty("sha").GetString());

        var properties = response.ResourceProperties();
        Assert.AreEqual("abc123", properties.GetProperty("sha").GetString());
    }

    [TestMethod]
    public async Task Is_idempotent_when_branch_already_exists()
    {
        var existingRef =
            """
            {
                "ref": "refs/heads/feature",
                "node_id": "n",
                "url": "https://api.github.com/repos/acme/widgets/git/refs/heads/feature",
                "object": {
                    "type": "commit",
                    "sha": "deadbeef",
                    "url": "https://api.github.com/repos/acme/widgets/git/commits/deadbeef"
                }
            }
            """;

        var mock = new MockHttpMessageHandler((request, _) =>
            request.Method == HttpMethod.Get
                ? MockHttpMessageHandler.Json(HttpStatusCode.OK, existingRef)
                : MockHttpMessageHandler.Json(HttpStatusCode.Created, existingRef));

        var handler = new BranchHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "Branch", new
        {
            owner = "acme",
            repo = "widgets",
            name = "feature",
            sourceBranch = "main",
        });

        Assert.IsNull(response.ErrorData);
        Assert.IsFalse(mock.Requests.Any(r => r.Method == HttpMethod.Post), "existing branch should not be recreated");

        var properties = response.ResourceProperties();
        Assert.AreEqual("deadbeef", properties.GetProperty("sha").GetString());
    }

    [TestMethod]
    public async Task Creates_branch_from_explicit_source_sha()
    {
        var newRef =
            """
            {
                "ref": "refs/heads/hotfix",
                "node_id": "n",
                "url": "https://api.github.com/repos/acme/widgets/git/refs/heads/hotfix",
                "object": {
                    "type": "commit",
                    "sha": "feed01",
                    "url": "https://api.github.com/repos/acme/widgets/git/commits/feed01"
                }
            }
            """;

        var mock = new MockHttpMessageHandler((request, _) =>
            request.Method == HttpMethod.Get
                ? MockHttpMessageHandler.Json(HttpStatusCode.NotFound, NotFoundJson)
                : MockHttpMessageHandler.Json(HttpStatusCode.Created, newRef));

        var handler = new BranchHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "Branch", new
        {
            owner = "acme",
            repo = "widgets",
            name = "hotfix",
            sourceSha = "feed01",
        });

        Assert.IsNull(response.ErrorData);

        // Only the existence check GET should occur; the source SHA is used directly.
        Assert.AreEqual(1, mock.Requests.Count(r => r.Method == HttpMethod.Get));

        var create = mock.Requests.Single(r => r.Method == HttpMethod.Post);
        var body = JsonSerializer.Deserialize<JsonElement>(create.Body);
        Assert.AreEqual("refs/heads/hotfix", body.GetProperty("ref").GetString());
        Assert.AreEqual("feed01", body.GetProperty("sha").GetString());
    }
}
