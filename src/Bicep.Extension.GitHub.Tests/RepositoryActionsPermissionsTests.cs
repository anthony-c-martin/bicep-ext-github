using System.Net;
using System.Text.Json;
using Bicep.Extension.Github.Handlers;

namespace Bicep.Extension.Github.Tests;

[TestClass]
public sealed class RepositoryActionsPermissionsTests
{
    [TestMethod]
    public async Task Sends_permissions_and_workflow_calls()
    {
        var mock = new MockHttpMessageHandler((_, _) => MockHttpMessageHandler.Empty(HttpStatusCode.NoContent));

        var handler = new RepositoryActionsPermissionsHandler { MessageHandlerOverride = mock };

        await HandlerHarness.CreateOrUpdateAsync(handler, "RepositoryActionsPermissions", new
        {
            owner = "acme",
            repo = "widgets",
            enabled = true,
            allowedActions = "selected",
            defaultWorkflowPermissions = "write",
            canApprovePullRequestReviews = true,
        });

        Assert.AreEqual(2, mock.Requests.Count);

        var permissions = mock.Requests[0];
        Assert.AreEqual(HttpMethod.Put, permissions.Method);
        Assert.AreEqual("/repos/acme/widgets/actions/permissions", permissions.Uri.AbsolutePath);
        var permissionsBody = JsonSerializer.Deserialize<JsonElement>(permissions.Body);
        Assert.IsTrue(permissionsBody.GetProperty("enabled").GetBoolean());
        Assert.AreEqual("selected", permissionsBody.GetProperty("allowed_actions").GetString());

        var workflow = mock.Requests[1];
        Assert.AreEqual(HttpMethod.Put, workflow.Method);
        Assert.AreEqual("/repos/acme/widgets/actions/permissions/workflow", workflow.Uri.AbsolutePath);
        var workflowBody = JsonSerializer.Deserialize<JsonElement>(workflow.Body);
        Assert.AreEqual("write", workflowBody.GetProperty("default_workflow_permissions").GetString());
        Assert.IsTrue(workflowBody.GetProperty("can_approve_pull_request_reviews").GetBoolean());
    }

    [TestMethod]
    public async Task Stops_after_first_call_fails()
    {
        var forbidden =
            """
            {
                "message": "Forbidden"
            }
            """;

        var mock = new MockHttpMessageHandler((_, _) =>
            MockHttpMessageHandler.Json(HttpStatusCode.Forbidden, forbidden));

        var handler = new RepositoryActionsPermissionsHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "RepositoryActionsPermissions", new
        {
            owner = "acme",
            repo = "widgets",
            enabled = true,
            allowedActions = "all",
            defaultWorkflowPermissions = "read",
            canApprovePullRequestReviews = false,
        });

        Assert.IsNotNull(response.ErrorData);
        Assert.AreEqual(1, mock.Requests.Count);
    }
}
