using System.Net;
using Bicep.Extension.Github.Handlers;

namespace Bicep.Extension.Github.Tests;

[TestClass]
public sealed class TeamRepositoryPermissionTests
{
    [TestMethod]
    public async Task Grants_team_permission_on_repository()
    {
        var mock = new MockHttpMessageHandler((_, _) => MockHttpMessageHandler.Empty(HttpStatusCode.NoContent));

        var handler = new TeamRepositoryPermissionHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "TeamRepositoryPermission", new
        {
            org = "acme",
            teamSlug = "platform-engineering",
            owner = "acme",
            repo = "widgets",
            permission = "maintain",
        });

        Assert.IsNull(response.ErrorData);

        var put = mock.Requests.Single(r => r.Method == HttpMethod.Put);
        Assert.AreEqual("/orgs/acme/teams/platform-engineering/repos/acme/widgets", put.Uri.AbsolutePath);
    }
}
