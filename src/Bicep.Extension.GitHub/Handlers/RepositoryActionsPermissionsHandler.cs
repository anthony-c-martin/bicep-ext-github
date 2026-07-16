using System.Text.Json.Nodes;
using Bicep.Local.Extension.Host.Handlers;

namespace Bicep.Extension.Github.Handlers;

public class RepositoryActionsPermissionsHandler : GithubResourceHandlerBase<RepositoryActionsPermissions, RepositoryActionsPermissionsIdentifiers>
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
            using var httpClient = CreateHttpClient(request.Config!.Token);

            var permissionsPayload = new JsonObject
            {
                ["enabled"] = request.Properties.Enabled,
                ["allowed_actions"] = request.Properties.AllowedActions,
            };

            var permissionsPath = $"repos/{request.Properties.Owner}/{request.Properties.Repo}/actions/permissions";
            using (var response = await httpClient.PutAsync(permissionsPath, BuildContent(permissionsPayload), cancellationToken))
            {
                await EnsureSuccess(response, "RepositoryActionsPermissions");
            }

            var workflowPayload = new JsonObject
            {
                ["default_workflow_permissions"] = request.Properties.DefaultWorkflowPermissions,
                ["can_approve_pull_request_reviews"] = request.Properties.CanApprovePullRequestReviews,
            };

            var workflowPath = $"repos/{request.Properties.Owner}/{request.Properties.Repo}/actions/permissions/workflow";
            using (var response = await httpClient.PutAsync(workflowPath, BuildContent(workflowPayload), cancellationToken))
            {
                await EnsureSuccess(response, "RepositoryActionsPermissions");
            }

            return GetResponse(request);
        });

    protected override RepositoryActionsPermissionsIdentifiers GetIdentifiers(RepositoryActionsPermissions properties)
        => new()
        {
            Owner = properties.Owner,
            Repo = properties.Repo,
        };
}
