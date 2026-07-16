using System.Net;
using System.Text;
using System.Text.Json;
using Bicep.Extension.Github.Handlers;

namespace Bicep.Extension.Github.Tests;

[TestClass]
public sealed class GitHubFileTests
{
    private const string NotFoundJson =
        """
        {
            "message": "Not Found"
        }
        """;

    private const string ChangeSetJson =
        """
        {
            "content": {
                "name": "README.md",
                "path": "README.md",
                "sha": "blobsha",
                "html_url": "https://github.com/acme/widgets/blob/main/README.md",
                "download_url": "https://raw.githubusercontent.com/acme/widgets/main/README.md"
            },
            "commit": {
                "sha": "commitsha"
            }
        }
        """;

    [TestMethod]
    public async Task Creates_file_when_absent()
    {
        var mock = new MockHttpMessageHandler((request, _) =>
            request.Method == HttpMethod.Get
                ? MockHttpMessageHandler.Json(HttpStatusCode.NotFound, NotFoundJson)
                : MockHttpMessageHandler.Json(HttpStatusCode.Created, ChangeSetJson));

        var handler = new GitHubFileHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "GitHubFile", new
        {
            owner = "acme",
            repo = "widgets",
            path = "README.md",
            content = "hello world",
            commitMessage = "add readme",
        });

        Assert.IsNull(response.ErrorData);

        var create = mock.Requests.Single(r => r.Method == HttpMethod.Put);
        Assert.AreEqual("/repos/acme/widgets/contents/README.md", create.Uri.AbsolutePath);

        var body = JsonSerializer.Deserialize<JsonElement>(create.Body);
        Assert.AreEqual("add readme", body.GetProperty("message").GetString());
        var content = Encoding.UTF8.GetString(Convert.FromBase64String(body.GetProperty("content").GetString()!));
        Assert.AreEqual("hello world", content);

        var properties = response.ResourceProperties();
        Assert.AreEqual("blobsha", properties.GetProperty("sha").GetString());
    }
}
