using System.Net;
using System.Text.Json;
using Bicep.Extension.Github.Handlers;

namespace Bicep.Extension.Github.Tests;

[TestClass]
public sealed class OrganizationActionsVariableTests
{
    private const string NotFoundJson =
        """
        {
            "message": "Not Found"
        }
        """;

    [TestMethod]
    public async Task Creates_variable_with_selected_repository_ids()
    {
        var mock = new MockHttpMessageHandler((request, _) =>
            request.Method == HttpMethod.Get
                ? MockHttpMessageHandler.Json(HttpStatusCode.NotFound, NotFoundJson)
                : MockHttpMessageHandler.Json(HttpStatusCode.Created, "{}"));

        var handler = new OrganizationActionsVariableHandler { MessageHandlerOverride = mock };

        await HandlerHarness.CreateOrUpdateAsync(handler, "OrganizationActionsVariable", new
        {
            org = "acme",
            name = "REGISTRY",
            value = "ghcr.io",
            visibility = "selected",
            selectedRepositoryIds = new[] { 1, 2, 3 },
        });

        var create = mock.Requests[1];
        Assert.AreEqual(HttpMethod.Post, create.Method);
        Assert.AreEqual("/orgs/acme/actions/variables", create.Uri.AbsolutePath);

        var body = JsonSerializer.Deserialize<JsonElement>(create.Body);
        Assert.AreEqual("selected", body.GetProperty("visibility").GetString());
        var ids = body.GetProperty("selected_repository_ids").EnumerateArray().Select(e => e.GetInt32()).ToArray();
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, ids);
    }

    [TestMethod]
    public async Task Omits_selected_repository_ids_when_visibility_is_all()
    {
        var mock = new MockHttpMessageHandler((request, _) =>
            request.Method == HttpMethod.Get
                ? MockHttpMessageHandler.Json(HttpStatusCode.NotFound, NotFoundJson)
                : MockHttpMessageHandler.Json(HttpStatusCode.Created, "{}"));

        var handler = new OrganizationActionsVariableHandler { MessageHandlerOverride = mock };

        await HandlerHarness.CreateOrUpdateAsync(handler, "OrganizationActionsVariable", new
        {
            org = "acme",
            name = "REGISTRY",
            value = "ghcr.io",
            visibility = "all",
        });

        var body = JsonSerializer.Deserialize<JsonElement>(mock.Requests[1].Body);
        Assert.AreEqual("all", body.GetProperty("visibility").GetString());
        Assert.IsFalse(body.TryGetProperty("selected_repository_ids", out _));
    }

    [TestMethod]
    public async Task Updates_variable_when_present()
    {
        var existing =
            """
            {
                "name": "REGISTRY",
                "value": "old"
            }
            """;

        var mock = new MockHttpMessageHandler((request, _) =>
            request.Method == HttpMethod.Get
                ? MockHttpMessageHandler.Json(HttpStatusCode.OK, existing)
                : MockHttpMessageHandler.Json(HttpStatusCode.NoContent, "{}"));

        var handler = new OrganizationActionsVariableHandler { MessageHandlerOverride = mock };

        await HandlerHarness.CreateOrUpdateAsync(handler, "OrganizationActionsVariable", new
        {
            org = "acme",
            name = "REGISTRY",
            value = "ghcr.io",
            visibility = "all",
        });

        var update = mock.Requests[1];
        Assert.AreEqual(HttpMethod.Patch, update.Method);
        Assert.AreEqual("/orgs/acme/actions/variables/REGISTRY", update.Uri.AbsolutePath);
    }
}
