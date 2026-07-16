using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using Bicep.Local.Extension.Host.Handlers;
using Octokit;
using Octokit.Internal;

namespace Bicep.Extension.Github.Handlers;

public abstract class GithubResourceHandlerBase<TProperties, TIdentifiers> : TypedResourceHandler<TProperties, TIdentifiers, Configuration>
    where TProperties : class
    where TIdentifiers : class
{
    /// <summary>
    /// Test seam. When set, all outgoing HTTP requests (both Octokit and raw <see cref="HttpClient"/>)
    /// are routed through this message handler, allowing responses to be mocked.
    /// </summary>
    internal HttpMessageHandler? MessageHandlerOverride { get; set; }

    protected async Task<ResourceResponse> HandleRequest(ResourceBase resource, Func<IGitHubClient, Task<ResourceResponse>> onExecuteFunc)
    {
        var client = CreateGitHubClient(resource.Config!.Token);

        try
        {
            return await onExecuteFunc(client);
        }
        catch (ApiException exception) when (exception.ApiError is { } apiError)
        {
            var errorDetails = (apiError.Errors ?? [])
                .Select(error => new ErrorDetail
                {
                    Code = error.Code,
                    Message = error.Message,
                    Target = error.Field
                });

            throw new ResourceErrorException("ApiError", apiError.Message, details: [..errorDetails]);
        }
        catch (ApiException exception)
        {
            throw new ResourceErrorException(
                "GitHubApiError",
                $"GitHub API request failed with status {(int)exception.StatusCode}: {exception.Message}");
        }
        catch (ResourceErrorException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new ResourceErrorException(
                "UnhandledException",
                exception.Message,
                details:
                [
                    new ErrorDetail
                    {
                        Code = exception.GetType().Name,
                        Message = exception.ToString(),
                    }
                ]);
        }
    }

    private IGitHubClient CreateGitHubClient(string token)
    {
        var productHeader = new Octokit.ProductHeaderValue("Bicep.LocalDeploy");
        var credentialStore = new InMemoryCredentialStore(new Credentials(token));

        if (MessageHandlerOverride is { } handler)
        {
            var connection = new Connection(
                productHeader,
                GitHubClient.GitHubApiUrl,
                credentialStore,
                new HttpClientAdapter(() => handler),
                new SimpleJsonSerializer());

            return new GitHubClient(connection);
        }

        return new GitHubClient(productHeader, credentialStore);
    }

    protected HttpClient CreateHttpClient(string token)
    {
        // When a mock handler is supplied, do not let the HttpClient dispose it so tests can reuse it.
        var client = MessageHandlerOverride is { } handler
            ? new HttpClient(handler, disposeHandler: false)
            : new HttpClient();

        client.BaseAddress = new Uri("https://api.github.com/");
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Bicep.LocalDeploy", "1.0"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

        return client;
    }

    protected static StringContent BuildContent(JsonObject payload)
        => new(payload.ToJsonString(), Encoding.UTF8, "application/json");

    protected static async Task EnsureSuccess(HttpResponseMessage response, string target)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        throw new ResourceErrorException(
            "GitHubApiError",
            $"{target} API request failed with status {(int)response.StatusCode}: {body}");
    }
}

