using System.Text.Json.Nodes;
using Bicep.Local.Extension.Host.Handlers;
using Sodium;

namespace Bicep.Extension.Github.Handlers;

public class DependabotSecretHandler : GithubResourceHandlerBase<DependabotSecret, DependabotSecretIdentifiers>
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

            var publicKeyPath = $"repos/{request.Properties.Owner}/{request.Properties.Repo}/dependabot/secrets/public-key";
            using var publicKeyResponse = await httpClient.GetAsync(publicKeyPath, cancellationToken);
            await EnsureSuccess(publicKeyResponse, "DependabotSecret");

            var publicKeyBody = await publicKeyResponse.Content.ReadAsStringAsync(cancellationToken);
            var publicKey = JsonNode.Parse(publicKeyBody)!;
            var keyId = publicKey["key_id"]!.GetValue<string>();
            var key = publicKey["key"]!.GetValue<string>();

            var publicKeyEncoded = Convert.FromBase64String(key);
            var sealedPublicKeyBox = SealedPublicKeyBox.Create(request.Properties.Value!, publicKeyEncoded);
            var encryptedSecret = Convert.ToBase64String(sealedPublicKeyBox);

            var payload = new JsonObject
            {
                ["encrypted_value"] = encryptedSecret,
                ["key_id"] = keyId,
            };

            var secretPath = $"repos/{request.Properties.Owner}/{request.Properties.Repo}/dependabot/secrets/{Uri.EscapeDataString(request.Properties.Name)}";
            using var response = await httpClient.PutAsync(secretPath, BuildContent(payload), cancellationToken);
            await EnsureSuccess(response, "DependabotSecret");

            // don't return the value to Bicep
            request.Properties.Value = null;

            return GetResponse(request);
        });

    protected override DependabotSecretIdentifiers GetIdentifiers(DependabotSecret properties)
        => new()
        {
            Owner = properties.Owner,
            Repo = properties.Repo,
            Name = properties.Name,
        };
}
