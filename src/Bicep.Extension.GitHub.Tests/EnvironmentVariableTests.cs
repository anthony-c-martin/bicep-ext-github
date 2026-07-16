using System.Net;
using System.Text.Json;
using Bicep.Extension.Github.Handlers;

namespace Bicep.Extension.Github.Tests;

[TestClass]
public sealed class EnvironmentVariableTests
{
    private const string NotFoundJson =
        """
        {
            "message": "Not Found"
        }
        """;

    private static object SampleProperties() => new
    {
        owner = "acme",
        repo = "widgets",
        environment = "production",
        name = "REGION",
        value = "westus",
    };

    [TestMethod]
    public async Task Creates_variable_when_absent()
    {
        var mock = new MockHttpMessageHandler((request, _) =>
            request.Method == HttpMethod.Get
                ? MockHttpMessageHandler.Json(HttpStatusCode.NotFound, NotFoundJson)
                : MockHttpMessageHandler.Json(HttpStatusCode.Created, "{}"));

        var handler = new EnvironmentVariableHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "EnvironmentVariable", SampleProperties());

        Assert.IsNull(response.ErrorData);
        Assert.AreEqual(2, mock.Requests.Count);

        var create = mock.Requests[1];
        Assert.AreEqual(HttpMethod.Post, create.Method);
        Assert.AreEqual("/repos/acme/widgets/environments/production/variables", create.Uri.AbsolutePath);

        var body = JsonSerializer.Deserialize<JsonElement>(create.Body);
        Assert.AreEqual("REGION", body.GetProperty("name").GetString());
        Assert.AreEqual("westus", body.GetProperty("value").GetString());
    }

    [TestMethod]
    public async Task Updates_variable_when_present()
    {
        var existing =
            """
            {
                "name": "REGION",
                "value": "eastus"
            }
            """;

        var mock = new MockHttpMessageHandler((request, _) =>
            request.Method == HttpMethod.Get
                ? MockHttpMessageHandler.Json(HttpStatusCode.OK, existing)
                : MockHttpMessageHandler.Json(HttpStatusCode.NoContent, "{}"));

        var handler = new EnvironmentVariableHandler { MessageHandlerOverride = mock };

        await HandlerHarness.CreateOrUpdateAsync(handler, "EnvironmentVariable", SampleProperties());

        var update = mock.Requests[1];
        Assert.AreEqual(HttpMethod.Patch, update.Method);
        Assert.AreEqual("/repos/acme/widgets/environments/production/variables/REGION", update.Uri.AbsolutePath);
    }

    [TestMethod]
    public async Task Surfaces_api_failure_as_error_data()
    {
        var validationFailed =
            """
            {
                "message": "Validation failed"
            }
            """;

        var mock = new MockHttpMessageHandler((request, _) =>
            request.Method == HttpMethod.Get
                ? MockHttpMessageHandler.Json(HttpStatusCode.NotFound, NotFoundJson)
                : MockHttpMessageHandler.Json(HttpStatusCode.UnprocessableEntity, validationFailed));

        var handler = new EnvironmentVariableHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "EnvironmentVariable", SampleProperties());

        Assert.IsNotNull(response.ErrorData);
        Assert.AreEqual("GitHubApiError", response.ErrorData.Error.Code);
    }
}
