using System.Net;
using System.Text.Json;
using Bicep.Extension.Github.Handlers;

namespace Bicep.Extension.Github.Tests;

[TestClass]
public sealed class RepositoryTests
{
    private const string UserJson =
        """
        {
            "login": "acme",
            "id": 1
        }
        """;

    private const string RepositoryJson =
        """
        {
            "id": 100,
            "name": "widgets",
            "owner": {
                "login": "acme",
                "id": 1
            }
        }
        """;

    [TestMethod]
    public async Task Updates_repository_when_it_exists()
    {
        var mock = new MockHttpMessageHandler((request, _) =>
        {
            if (request.RequestUri!.AbsolutePath == "/user")
            {
                return MockHttpMessageHandler.Json(HttpStatusCode.OK, UserJson);
            }

            return MockHttpMessageHandler.Json(HttpStatusCode.OK, RepositoryJson);
        });

        var handler = new RepositoryHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "Repository", new
        {
            owner = "acme",
            name = "widgets",
            description = "A repo",
        });

        Assert.IsNull(response.ErrorData);

        var edit = mock.Requests.Single(r => r.Method == HttpMethod.Patch);
        Assert.AreEqual("/repos/acme/widgets", edit.Uri.AbsolutePath);
    }

    [TestMethod]
    public async Task Creates_repository_for_current_user_when_absent()
    {
        var mock = new MockHttpMessageHandler((request, _) =>
        {
            if (request.RequestUri!.AbsolutePath == "/user")
            {
                return MockHttpMessageHandler.Json(HttpStatusCode.OK, UserJson);
            }

            return request.Method == HttpMethod.Patch
                ? MockHttpMessageHandler.Json(HttpStatusCode.NotFound, """{"message":"Not Found"}""")
                : MockHttpMessageHandler.Json(HttpStatusCode.Created, RepositoryJson);
        });

        var handler = new RepositoryHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "Repository", new
        {
            owner = "acme",
            name = "widgets",
            description = "A repo",
        });

        Assert.IsNull(response.ErrorData);

        var create = mock.Requests.Single(r => r.Method == HttpMethod.Post);
        Assert.AreEqual("/user/repos", create.Uri.AbsolutePath);
    }
}
