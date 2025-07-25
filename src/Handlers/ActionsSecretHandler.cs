using System.Text.Json.Nodes;
using Bicep.Local.Extension.Host.Handlers;
using Octokit;
using Sodium;

namespace Bicep.Extension.Github.Handlers;

public class ActionsSecretHandler : GithubResourceHandlerBase<ActionsSecret, ActionsSecretIdentifiers>
{
    protected override Task<ResourceResponse> Preview(ResourceRequest request, CancellationToken cancellationToken)
        => HandleRequest(request, async client =>
        {
            await Task.CompletedTask;

            return GetResponse(request);
        });

    protected override Task<ResourceResponse> CreateOrUpdate(ResourceRequest request, CancellationToken cancellationToken)
        => HandleRequest(request, async client =>
        {
            var publicKey = await client.Repository.Actions.Secrets.GetPublicKey(request.Properties.Owner, request.Properties.Repo);

            var publicKeyEncoded = Convert.FromBase64String(publicKey.Key);
            var sealedPublicKeyBox = SealedPublicKeyBox.Create(request.Properties.Value!, publicKeyEncoded);
            var encryptedSecret = Convert.ToBase64String(sealedPublicKeyBox);

            await client.Repository.Actions.Secrets.CreateOrUpdate(
                request.Properties.Owner,
                request.Properties.Repo,
                request.Properties.Name,
                new()
                {
                    EncryptedValue = encryptedSecret,
                    KeyId = publicKey.KeyId,
                });

            // don't return the value to Bicep
            request.Properties.Value = null;

            return GetResponse(request);
        });

    protected override ActionsSecretIdentifiers GetIdentifiers(ActionsSecret properties)
        => new()
        {
            Owner = properties.Owner,
            Repo = properties.Repo,
            Name = properties.Name,
        };
}