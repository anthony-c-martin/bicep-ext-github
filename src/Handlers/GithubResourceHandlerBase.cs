using Bicep.Local.Extension.Host.Handlers;
using Octokit;
using Octokit.Internal;

namespace Bicep.Extension.Github.Handlers;

public abstract class GithubResourceHandlerBase<TProperties, TIdentifiers> : TypedResourceHandler<TProperties, TIdentifiers, Configuration>
    where TProperties : class
    where TIdentifiers : class
{
    protected async Task<ResourceResponse> HandleRequest(ResourceBase resource, Func<GitHubClient, Task<ResourceResponse>> onExecuteFunc)
    {
        var credentials = new Credentials(resource.Config!.Token);
        var client = new GitHubClient(new ProductHeaderValue("Bicep.LocalDeploy"), new InMemoryCredentialStore(credentials));

        try
        {
            return await onExecuteFunc(client);
        }
        catch (ApiException exception) when (exception.ApiError is { } apiError)
        {
            var errorDetails = apiError.Errors
                .Select(error => new ErrorDetail
                {
                    Code = error.Code,
                    Message = error.Message,
                    Target = error.Field
                });

            throw new ResourceErrorException("ApiError", apiError.Message, details: [..errorDetails]);
        }
    }
}
