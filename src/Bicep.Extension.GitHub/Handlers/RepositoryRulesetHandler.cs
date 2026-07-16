using System.Text.Json;
using System.Text.Json.Nodes;
using Bicep.Local.Extension.Host.Handlers;

namespace Bicep.Extension.Github.Handlers;

public class RepositoryRulesetHandler : GithubResourceHandlerBase<RepositoryRuleset, RepositoryRulesetIdentifiers>
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

            var listPath = $"repos/{request.Properties.Owner}/{request.Properties.Repo}/rulesets?includes_parents=false";
            using var listResponse = await httpClient.GetAsync(listPath, cancellationToken);
            await EnsureSuccess(listResponse, "RepositoryRuleset");

            var listBody = await listResponse.Content.ReadAsStringAsync(cancellationToken);
            var rulesets = JsonNode.Parse(listBody)?.AsArray() ?? [];

            long? existingId = null;
            foreach (var node in rulesets)
            {
                if (node is not JsonObject ruleset)
                {
                    continue;
                }

                if (string.Equals(ruleset["name"]?.GetValue<string>(), request.Properties.Name, StringComparison.Ordinal))
                {
                    existingId = ruleset["id"]?.GetValue<long>();
                    break;
                }
            }

            var payload = new JsonObject
            {
                ["name"] = request.Properties.Name,
                ["target"] = request.Properties.Target,
                ["enforcement"] = request.Properties.Enforcement,
                ["conditions"] = JsonSerializer.SerializeToNode(request.Properties.Conditions),
                ["rules"] = JsonSerializer.SerializeToNode(request.Properties.Rules),
            };

            // Support both typed BypassActors array and legacy BypassActorsJson
            if (request.Properties.BypassActors is { Length: > 0 })
            {
                payload["bypass_actors"] = JsonSerializer.SerializeToNode(request.Properties.BypassActors);
            }
            else if (!string.IsNullOrWhiteSpace(request.Properties.BypassActorsJson))
            {
                payload["bypass_actors"] = ParseRequiredJsonArray(request.Properties.BypassActorsJson, "bypassActorsJson");
            }

            if (existingId is null)
            {
                var createPath = $"repos/{request.Properties.Owner}/{request.Properties.Repo}/rulesets";
                using var createResponse = await httpClient.PostAsync(createPath, BuildContent(payload), cancellationToken);
                await EnsureSuccess(createResponse, "RepositoryRuleset");
            }
            else
            {
                var updatePath = $"repos/{request.Properties.Owner}/{request.Properties.Repo}/rulesets/{existingId.Value}";
                using var updateResponse = await httpClient.PutAsync(updatePath, BuildContent(payload), cancellationToken);
                await EnsureSuccess(updateResponse, "RepositoryRuleset");
            }

            return GetResponse(request);
        });

    protected override RepositoryRulesetIdentifiers GetIdentifiers(RepositoryRuleset properties)
        => new()
        {
            Owner = properties.Owner,
            Repo = properties.Repo,
            Name = properties.Name,
        };

    private static JsonArray ParseRequiredJsonArray(string json, string propertyName)
    {
        var node = JsonNode.Parse(json);
        if (node is not JsonArray arr)
        {
            throw new ResourceErrorException("InvalidInput", $"Property '{propertyName}' must be a JSON array string.");
        }

        return arr;
    }
}
