using System.Net;
using System.Text.Json.Nodes;
using Bicep.Local.Extension.Host.Handlers;

namespace Bicep.Extension.Github.Handlers;

public class EnvironmentVariableHandler : GithubResourceHandlerBase<EnvironmentVariable, EnvironmentVariableIdentifiers>
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

            var environment = Uri.EscapeDataString(request.Properties.Environment);
            var basePath = $"repos/{request.Properties.Owner}/{request.Properties.Repo}/environments/{environment}/variables";

            var payload = new JsonObject
            {
                ["name"] = request.Properties.Name,
                ["value"] = request.Properties.Value,
            };

            var existsPath = $"{basePath}/{Uri.EscapeDataString(request.Properties.Name)}";
            using var existing = await httpClient.GetAsync(existsPath, cancellationToken);

            HttpResponseMessage response;
            if (existing.StatusCode == HttpStatusCode.NotFound)
            {
                response = await httpClient.PostAsync(basePath, BuildContent(payload), cancellationToken);
            }
            else
            {
                response = await httpClient.PatchAsync(existsPath, BuildContent(payload), cancellationToken);
            }

            using (response)
            {
                await EnsureSuccess(response, "EnvironmentVariable");
            }

            return GetResponse(request);
        });

    protected override EnvironmentVariableIdentifiers GetIdentifiers(EnvironmentVariable properties)
        => new()
        {
            Owner = properties.Owner,
            Repo = properties.Repo,
            Environment = properties.Environment,
            Name = properties.Name,
        };
}
