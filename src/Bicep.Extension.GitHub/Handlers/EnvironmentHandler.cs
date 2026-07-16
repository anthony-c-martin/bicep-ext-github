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
            
            var deploymentBranchPolicy = new JsonObject
            {
                ["protected_branches"] = request.Properties.ProtectedBranches,
                ["custom_branch_policies"] = request.Properties.CustomBranchPolicies,
            };
            
            if (request.Properties.DeploymentBranchPolicyEnvironments is { Length: > 0 })
            {
                deploymentBranchPolicy["environments"] = JsonNode.Parse(
                    JsonSerializer.Serialize(request.Properties.DeploymentBranchPolicyEnvironments));
            }

            var payload = new JsonObject
            {
                ["wait_timer"] = request.Properties.WaitTimer,
                ["prevent_self_review"] = request.Properties.PreventSelfReview,
                ["deployment_branch_policy"] = deploymentBranchPolicy,
            };

            if (request.Properties.Reviewers is { Length: > 0 })
            {
                var reviewersArray = new JsonArray();
                foreach (var reviewer in request.Properties.Reviewers)
                {
                    reviewersArray.Add(new JsonObject
                    {
                        ["id"] = reviewer.Id,
                        ["type"] = reviewer.Type,
                    });
                }
                payload["reviewers"] = reviewersArray;
            }

            using var httpClient = CreateHttpClient(request.Config!.Token);
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
}
