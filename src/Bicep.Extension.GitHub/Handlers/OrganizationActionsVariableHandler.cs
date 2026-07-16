using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Bicep.Local.Extension.Host.Handlers;

namespace Bicep.Extension.Github.Handlers;

public class OrganizationActionsVariableHandler : GithubResourceHandlerBase<OrganizationActionsVariable, OrganizationActionsVariableIdentifiers>
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

            var payload = new JsonObject
            {
                ["name"] = request.Properties.Name,
                ["value"] = request.Properties.Value,
                ["visibility"] = request.Properties.Visibility,
            };

            if (string.Equals(request.Properties.Visibility, "selected", StringComparison.OrdinalIgnoreCase)
                && request.Properties.SelectedRepositoryIds is { Length: > 0 })
            {
                payload["selected_repository_ids"] = JsonNode.Parse(
                    JsonSerializer.Serialize(request.Properties.SelectedRepositoryIds));
            }

            var existsPath = $"orgs/{request.Properties.Org}/actions/variables/{Uri.EscapeDataString(request.Properties.Name)}";
            using var existing = await httpClient.GetAsync(existsPath, cancellationToken);

            HttpResponseMessage response;
            if (existing.StatusCode == HttpStatusCode.NotFound)
            {
                var createPath = $"orgs/{request.Properties.Org}/actions/variables";
                response = await httpClient.PostAsync(createPath, BuildContent(payload), cancellationToken);
            }
            else
            {
                response = await httpClient.PatchAsync(existsPath, BuildContent(payload), cancellationToken);
            }

            using (response)
            {
                await EnsureSuccess(response, "OrganizationActionsVariable");
            }

            return GetResponse(request);
        });

    protected override OrganizationActionsVariableIdentifiers GetIdentifiers(OrganizationActionsVariable properties)
        => new()
        {
            Org = properties.Org,
            Name = properties.Name,
        };
}
