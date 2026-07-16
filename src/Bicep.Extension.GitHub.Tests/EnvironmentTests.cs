using System.Net;
using System.Text.Json;
using Bicep.Extension.Github.Handlers;

namespace Bicep.Extension.Github.Tests;

[TestClass]
public sealed class EnvironmentTests
{
    [TestMethod]
    public async Task Creates_or_updates_environment()
    {
        var mock = new MockHttpMessageHandler((_, _) => MockHttpMessageHandler.Json(HttpStatusCode.OK, "{}"));

        var handler = new EnvironmentHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "Environment", new
        {
            owner = "acme",
            repo = "widgets",
            name = "production",
            waitTimer = 30,
            preventSelfReview = true,
            protectedBranches = true,
            customBranchPolicies = false,
        });

        Assert.IsNull(response.ErrorData);

        var put = mock.Requests.Single(r => r.Method == HttpMethod.Put);
        Assert.AreEqual("/repos/acme/widgets/environments/production", put.Uri.AbsolutePath);

        var body = JsonSerializer.Deserialize<JsonElement>(put.Body);
        Assert.AreEqual(30, body.GetProperty("wait_timer").GetInt32());
        Assert.IsTrue(body.GetProperty("prevent_self_review").GetBoolean());
        Assert.IsTrue(body.GetProperty("deployment_branch_policy").GetProperty("protected_branches").GetBoolean());
    }
}
