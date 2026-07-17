using System.Net;
using System.Text.Json;
using Bicep.Extension.Github.Handlers;

namespace Bicep.Extension.Github.Tests;

[TestClass]
public sealed class RepositoryRulesetTests
{
    [TestMethod]
    public async Task Creates_ruleset_when_absent()
    {
        var mock = new MockHttpMessageHandler((request, _) =>
            request.Method == HttpMethod.Get
                ? MockHttpMessageHandler.Json(HttpStatusCode.OK, "[]")
                : MockHttpMessageHandler.Json(HttpStatusCode.Created, """{"id":1,"name":"main"}"""));

        var handler = new RepositoryRulesetHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "RepositoryRuleset", new
        {
            owner = "acme",
            repo = "widgets",
            name = "main",
            target = "branch",
            enforcement = "active",
            conditions = new Dictionary<string, object>
            {
                ["refName"] = new { include = new[] { "~DEFAULT_BRANCH" }, exclude = Array.Empty<string>() },
            },
            rules = new[]
            {
                new Dictionary<string, object> { ["type"] = "deletion" },
            },
        });

        Assert.IsNull(response.ErrorData);

        var create = mock.Requests.Single(r => r.Method == HttpMethod.Post);
        Assert.AreEqual("/repos/acme/widgets/rulesets", create.Uri.AbsolutePath);

        var body = JsonSerializer.Deserialize<JsonElement>(create.Body);
        Assert.AreEqual("main", body.GetProperty("name").GetString());
        Assert.AreEqual("branch", body.GetProperty("target").GetString());
        Assert.AreEqual("active", body.GetProperty("enforcement").GetString());
    }

    [TestMethod]
    public async Task Updates_ruleset_when_present()
    {
        var mock = new MockHttpMessageHandler((request, _) =>
        {
            if (request.Method == HttpMethod.Get)
            {
                return MockHttpMessageHandler.Json(HttpStatusCode.OK, """[{"id":7,"name":"main"}]""");
            }

            return MockHttpMessageHandler.Json(HttpStatusCode.OK, """{"id":7,"name":"main"}""");
        });

        var handler = new RepositoryRulesetHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "RepositoryRuleset", new
        {
            owner = "acme",
            repo = "widgets",
            name = "main",
            target = "branch",
            enforcement = "active",
            conditions = new Dictionary<string, object>
            {
                ["refName"] = new { include = new[] { "~DEFAULT_BRANCH" }, exclude = Array.Empty<string>() },
            },
            rules = new[]
            {
                new Dictionary<string, object> { ["type"] = "deletion" },
            },
        });

        Assert.IsNull(response.ErrorData);

        var update = mock.Requests.Single(r => r.Method == HttpMethod.Put);
        Assert.AreEqual("/repos/acme/widgets/rulesets/7", update.Uri.AbsolutePath);
    }

    [TestMethod]
    public async Task Serializes_conditions_and_rules_using_snake_case()
    {
        var mock = new MockHttpMessageHandler((request, _) =>
            request.Method == HttpMethod.Get
                ? MockHttpMessageHandler.Json(HttpStatusCode.OK, "[]")
                : MockHttpMessageHandler.Json(HttpStatusCode.Created, """{"id":1,"name":"main"}"""));

        var handler = new RepositoryRulesetHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "RepositoryRuleset", new
        {
            owner = "acme",
            repo = "widgets",
            name = "main",
            target = "branch",
            enforcement = "active",
            conditions = new { refName = new { include = new[] { "~DEFAULT_BRANCH" }, exclude = Array.Empty<string>() } },
            rules = new object[]
            {
                new
                {
                    type = "pull_request",
                    parameters = new { dismissStaleReviewsOnPush = true, requiredApprovingReviewCount = 1 },
                },
                new
                {
                    type = "required_status_checks",
                    parameters = new
                    {
                        strictRequiredStatusChecksPolicy = true,
                        requiredStatusChecks = new[] { new { context = "build" } },
                    },
                },
            },
        });

        Assert.IsNull(response.ErrorData);

        var create = mock.Requests.Single(r => r.Method == HttpMethod.Post);
        var body = JsonSerializer.Deserialize<JsonElement>(create.Body);

        // ref_name must be populated (regression: camelCase input previously bound to null).
        var refName = body.GetProperty("conditions").GetProperty("ref_name");
        Assert.AreEqual("~DEFAULT_BRANCH", refName.GetProperty("include")[0].GetString());

        var rules = body.GetProperty("rules");

        var pullRequest = rules[0];
        Assert.AreEqual("pull_request", pullRequest.GetProperty("type").GetString());
        Assert.IsTrue(pullRequest.GetProperty("parameters").GetProperty("dismiss_stale_reviews_on_push").GetBoolean());
        Assert.AreEqual(1, pullRequest.GetProperty("parameters").GetProperty("required_approving_review_count").GetInt32());
        // A pull_request rule must not carry required_status_checks fields.
        Assert.IsFalse(pullRequest.GetProperty("parameters").TryGetProperty("required_status_checks", out _));

        var statusChecks = rules[1];
        Assert.AreEqual("required_status_checks", statusChecks.GetProperty("type").GetString());
        Assert.IsTrue(statusChecks.GetProperty("parameters").GetProperty("strict_required_status_checks_policy").GetBoolean());
        Assert.AreEqual("build", statusChecks.GetProperty("parameters").GetProperty("required_status_checks")[0].GetProperty("context").GetString());
    }
}
