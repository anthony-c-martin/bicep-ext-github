using System.Net;
using System.Text.Json;
using Bicep.Extension.Github.Handlers;

namespace Bicep.Extension.Github.Tests;

[TestClass]
public sealed class LabelTests
{
    private const string LabelJson =
        """
        {
            "id": 1,
            "node_id": "n",
            "url": "https://api.github.com/repos/acme/widgets/labels/bug",
            "name": "bug",
            "color": "ff0000",
            "default": false,
            "description": "A bug"
        }
        """;

    private const string NotFoundJson =
        """
        {
            "message": "Not Found"
        }
        """;

    [TestMethod]
    public async Task Creates_label_when_absent()
    {
        var mock = new MockHttpMessageHandler((request, _) =>
            request.Method == HttpMethod.Get
                ? MockHttpMessageHandler.Json(HttpStatusCode.NotFound, NotFoundJson)
                : MockHttpMessageHandler.Json(HttpStatusCode.Created, LabelJson));

        var handler = new LabelHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "Label", new
        {
            owner = "acme",
            repo = "widgets",
            name = "bug",
            color = "ff0000",
            description = "A bug",
        });

        Assert.IsNull(response.ErrorData);

        var create = mock.Requests.Single(r => r.Method == HttpMethod.Post);
        Assert.AreEqual("/repos/acme/widgets/labels", create.Uri.AbsolutePath);

        var body = JsonSerializer.Deserialize<JsonElement>(create.Body);
        Assert.AreEqual("bug", body.GetProperty("name").GetString());
        Assert.AreEqual("ff0000", body.GetProperty("color").GetString());
    }

    [TestMethod]
    public async Task Updates_label_when_present()
    {
        var existing =
            """
            {
                "id": 1,
                "node_id": "n",
                "url": "https://api.github.com/repos/acme/widgets/labels/bug",
                "name": "bug",
                "color": "cccccc",
                "default": false,
                "description": "old"
            }
            """;

        var mock = new MockHttpMessageHandler((request, _) =>
            request.Method == HttpMethod.Get
                ? MockHttpMessageHandler.Json(HttpStatusCode.OK, existing)
                : MockHttpMessageHandler.Json(HttpStatusCode.OK, LabelJson));

        var handler = new LabelHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "Label", new
        {
            owner = "acme",
            repo = "widgets",
            name = "bug",
            color = "ff0000",
            description = "A bug",
        });

        Assert.IsNull(response.ErrorData);

        // Octokit issues a non-GET call against the specific label to update it.
        var update = mock.Requests.Single(r =>
            r.Method != HttpMethod.Get && r.Uri.AbsolutePath == "/repos/acme/widgets/labels/bug");

        var body = JsonSerializer.Deserialize<JsonElement>(update.Body);
        Assert.AreEqual("ff0000", body.GetProperty("color").GetString());
    }
}
