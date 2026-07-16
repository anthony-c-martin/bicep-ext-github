using System.Net;
using System.Text.Json;
using Bicep.Extension.Github.Handlers;

namespace Bicep.Extension.Github.Tests;

[TestClass]
public sealed class CollaboratorTests
{
    [TestMethod]
    public async Task Adds_collaborator_with_permission()
    {
        var mock = new MockHttpMessageHandler((_, _) => MockHttpMessageHandler.Empty(HttpStatusCode.NoContent));

        var handler = new CollaboratorHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "Collaborator", new
        {
            owner = "acme",
            repo = "widgets",
            user = "octocat",
            permission = "push",
        });

        Assert.IsNull(response.ErrorData);

        var add = mock.Requests.Single();
        Assert.AreEqual(HttpMethod.Put, add.Method);
        Assert.AreEqual("/repos/acme/widgets/collaborators/octocat", add.Uri.AbsolutePath);

        var body = JsonSerializer.Deserialize<JsonElement>(add.Body);
        Assert.AreEqual("push", body.GetProperty("permission").GetString());
    }
}
