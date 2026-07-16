using System.Net;
using System.Text.Json;
using Bicep.Extension.Github.Handlers;

namespace Bicep.Extension.Github.Tests;

[TestClass]
public sealed class TeamMembershipTests
{
    private const string TeamJson =
        """
        {
            "id": 42,
            "slug": "platform-engineering",
            "name": "Platform Engineering",
            "url": "https://api.github.com/organizations/1/team/42"
        }
        """;

    private const string MembershipJson =
        """
        {
            "url": "https://api.github.com/organizations/1/team/42/memberships/octocat",
            "role": "maintainer",
            "state": "active"
        }
        """;

    [TestMethod]
    public async Task Adds_membership_with_role()
    {
        var mock = new MockHttpMessageHandler((request, _) =>
            request.Method == HttpMethod.Get
                ? MockHttpMessageHandler.Json(HttpStatusCode.OK, TeamJson)
                : MockHttpMessageHandler.Json(HttpStatusCode.OK, MembershipJson));

        var handler = new TeamMembershipHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "TeamMembership", new
        {
            org = "acme",
            teamSlug = "platform-engineering",
            username = "octocat",
            role = "maintainer",
        });

        Assert.IsNull(response.ErrorData);

        var put = mock.Requests.Single(r => r.Method == HttpMethod.Put);
        StringAssert.Contains(put.Uri.AbsolutePath, "/memberships/octocat");

        var body = JsonSerializer.Deserialize<JsonElement>(put.Body);
        Assert.AreEqual("maintainer", body.GetProperty("role").GetString());
    }
}
