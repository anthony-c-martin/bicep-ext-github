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
                ["ref_name"] = new { include = new[] { "~DEFAULT_BRANCH" }, exclude = Array.Empty<string>() },
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
                ["ref_name"] = new { include = new[] { "~DEFAULT_BRANCH" }, exclude = Array.Empty<string>() },
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
}
