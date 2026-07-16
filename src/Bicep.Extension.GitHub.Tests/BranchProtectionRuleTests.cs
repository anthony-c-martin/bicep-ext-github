using System.Net;
using System.Text.Json;
using Bicep.Extension.Github.Handlers;

namespace Bicep.Extension.Github.Tests;

[TestClass]
public sealed class BranchProtectionRuleTests
{
    [TestMethod]
    public async Task Updates_branch_protection_settings()
    {
        var mock = new MockHttpMessageHandler((_, _) => MockHttpMessageHandler.Json(HttpStatusCode.OK, "{}"));

        var handler = new BranchProtectionRuleHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "BranchProtectionRule", new
        {
            owner = "acme",
            repo = "widgets",
            branch = "main",
            requiredStatusCheckContexts = new[] { "build" },
            strictStatusChecks = true,
            enablePullRequestReviews = true,
            requiredApprovingReviewCount = 2,
            enforceAdmins = true,
        });

        Assert.IsNull(response.ErrorData);

        var update = mock.Requests.Single(r => r.Method == HttpMethod.Put);
        Assert.AreEqual("/repos/acme/widgets/branches/main/protection", update.Uri.AbsolutePath);

        var body = JsonSerializer.Deserialize<JsonElement>(update.Body);
        Assert.IsTrue(body.GetProperty("enforce_admins").GetBoolean());
        Assert.AreEqual(2, body.GetProperty("required_pull_request_reviews").GetProperty("required_approving_review_count").GetInt32());
    }
}
