using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Bicep.Local.Extension.Host.Handlers;

namespace Bicep.Extension.Github.Handlers;

public class EnvironmentHandler : GithubResourceHandlerBase<Environment, EnvironmentIdentifiers>
{
    protected override Task<ResourceResponse> Preview(ResourceRequest request, CancellationToken cancellationToken)
        => HandleRequest(request, async _ =>
        {
            await Task.CompletedTask;
            return GetResponse(request);
        });

    protected override Task<ResourceResponse> CreateOrUpdate(ResourceRequest request, CancellationToken cancellationToken)
        => HandleRequest(request, async _ =>
        {
            var path = $"repos/{request.Properties.Owner}/{request.Properties.Repo}/environments/{Uri.EscapeDataString(request.Properties.Name)}";
            var payload = new JsonObject
            {
                ["wait_timer"] = request.Properties.WaitTimer,
                ["prevent_self_review"] = request.Properties.PreventSelfReview,
                ["deployment_branch_policy"] = new JsonObject
                {
                    ["protected_branches"] = request.Properties.ProtectedBranches,
                    ["custom_branch_policies"] = request.Properties.CustomBranchPolicies,
                },
            };

            using var httpClient = CreateClient(request.Config!.Token);
            using var response = await httpClient.PutAsync(path, BuildContent(payload), cancellationToken);

            await EnsureSuccess(response, "Environment");

            return GetResponse(request);
        });

    protected override EnvironmentIdentifiers GetIdentifiers(Environment properties)
        => new()
        {
            Owner = properties.Owner,
            Repo = properties.Repo,
            Name = properties.Name,
        };

    private static HttpClient CreateClient(string token)
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri("https://api.github.com/"),
        };

        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Bicep.LocalDeploy", "1.0"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

        return client;
    }

    private static StringContent BuildContent(JsonObject payload)
        => new(payload.ToJsonString(), Encoding.UTF8, "application/json");

    private static async Task EnsureSuccess(HttpResponseMessage response, string target)
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
