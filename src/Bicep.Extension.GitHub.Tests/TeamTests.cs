using System.Net;
using System.Text.Json;
using Bicep.Extension.Github.Handlers;

namespace Bicep.Extension.Github.Tests;

[TestClass]
public sealed class TeamTests
{
    private const string TeamJson =
        """
        {
            "id": 42,
            "node_id": "n",
            "slug": "platform-engineering",
            "name": "Platform Engineering",
            "description": "Platform team",
            "privacy": "closed",
            "url": "https://api.github.com/organizations/1/team/42"
        }
        """;

    [TestMethod]
    public async Task Creates_team_when_absent()
    {
        var mock = new MockHttpMessageHandler((request, _) =>
            request.Method == HttpMethod.Get
                ? MockHttpMessageHandler.Json(HttpStatusCode.NotFound, """{"message":"Not Found"}""")
                : MockHttpMessageHandler.Json(HttpStatusCode.Created, TeamJson));

        var handler = new TeamHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "Team", new
        {
            org = "acme",
            name = "Platform Engineering",
            description = "Platform team",
            privacy = "closed",
        });

        Assert.IsNull(response.ErrorData);

        var create = mock.Requests.Single(r => r.Method == HttpMethod.Post);
        Assert.AreEqual("/orgs/acme/teams", create.Uri.AbsolutePath);

        var body = JsonSerializer.Deserialize<JsonElement>(create.Body);
        Assert.AreEqual("Platform Engineering", body.GetProperty("name").GetString());

        var properties = response.ResourceProperties();
        Assert.AreEqual("platform-engineering", properties.GetProperty("slug").GetString());
        Assert.AreEqual(42, properties.GetProperty("id").GetInt32());
    }

    [TestMethod]
    public async Task Updates_team_when_present()
    {
        var mock = new MockHttpMessageHandler((request, _) =>
            request.Method == HttpMethod.Get
                ? MockHttpMessageHandler.Json(HttpStatusCode.OK, TeamJson)
                : MockHttpMessageHandler.Json(HttpStatusCode.OK, TeamJson));

        var handler = new TeamHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "Team", new
        {
            org = "acme",
            name = "Platform Engineering",
            description = "Updated",
            privacy = "closed",
        });

        Assert.IsNull(response.ErrorData);

        var update = mock.Requests.Single(r => r.Method == HttpMethod.Patch);
        Assert.AreEqual("/orgs/acme/teams/platform-engineering", update.Uri.AbsolutePath);
    }
}
