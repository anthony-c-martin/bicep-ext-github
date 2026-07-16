using System.Net;
using System.Text.Json;
using Bicep.Extension.Github.Handlers;

namespace Bicep.Extension.Github.Tests;

[TestClass]
public sealed class ActionsVariableTests
{
    private const string VariableJson =
        """
        {
            "name": "REGION",
            "value": "westus"
        }
        """;

    private const string ConflictJson =
        """
        {
            "message": "variable REGION already exists"
        }
        """;

    [TestMethod]
    public async Task Creates_variable_when_absent()
    {
        var mock = new MockHttpMessageHandler((_, _) =>
            MockHttpMessageHandler.Json(HttpStatusCode.Created, VariableJson));

        var handler = new ActionsVariableHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "ActionsVariable", new
        {
            owner = "acme",
            repo = "widgets",
            name = "REGION",
            value = "westus",
        });

        Assert.IsNull(response.ErrorData);

        // Octokit's Create issues a POST and then a follow-up GET to return the created variable.
        var create = mock.Requests.Single(r => r.Method == HttpMethod.Post);
        Assert.AreEqual("/repos/acme/widgets/actions/variables", create.Uri.AbsolutePath);
    }

    [TestMethod]
    public async Task Updates_variable_when_create_conflicts()
    {
        var mock = new MockHttpMessageHandler((request, _) =>
            request.Method == HttpMethod.Post
                ? MockHttpMessageHandler.Json(HttpStatusCode.Conflict, ConflictJson)
                : MockHttpMessageHandler.Json(HttpStatusCode.NoContent, VariableJson));

        var handler = new ActionsVariableHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "ActionsVariable", new
        {
            owner = "acme",
            repo = "widgets",
            name = "REGION",
            value = "westus",
        });

        Assert.IsNull(response.ErrorData);
        Assert.IsTrue(mock.Requests.Any(r => r.Method == HttpMethod.Post));

        var update = mock.Requests.Single(r => r.Method == HttpMethod.Patch);
        Assert.AreEqual("/repos/acme/widgets/actions/variables/REGION", update.Uri.AbsolutePath);
    }
}
