using Bicep.Local.Extension.Host.Handlers;
using Octokit;
using Sodium;

namespace Bicep.Extension.Github.Handlers;

public class OrganizationActionsSecretHandler : GithubResourceHandlerBase<OrganizationActionsSecret, OrganizationActionsSecretIdentifiers>
{
    protected override Task<ResourceResponse> Preview(ResourceRequest request, CancellationToken cancellationToken)
        => HandleRequest(request, async _ =>
        {
            await Task.CompletedTask;
            return GetResponse(request);
        });

    protected override Task<ResourceResponse> CreateOrUpdate(ResourceRequest request, CancellationToken cancellationToken)
        => HandleRequest(request, async client =>
        {
            var publicKey = await client.Organization.Actions.Secrets.GetPublicKey(request.Properties.Org);

            var publicKeyEncoded = Convert.FromBase64String(publicKey.Key);
            var sealedPublicKeyBox = SealedPublicKeyBox.Create(request.Properties.Value!, publicKeyEncoded);
            var encryptedSecret = Convert.ToBase64String(sealedPublicKeyBox);

            var upsert = new UpsertOrganizationSecret
            {
                EncryptedValue = encryptedSecret,
                KeyId = publicKey.KeyId,
                Visibility = request.Properties.Visibility,
                SelectedRepositoriesIds = request.Properties.SelectedRepositoryIds?.Select(id => (long)id),
            };

            await client.Organization.Actions.Secrets.CreateOrUpdate(
                request.Properties.Org,
                request.Properties.Name,
                upsert);

            // don't return the value to Bicep
            request.Properties.Value = null;

            return GetResponse(request);
        });

    protected override OrganizationActionsSecretIdentifiers GetIdentifiers(OrganizationActionsSecret properties)
        => new()
        {
            Org = properties.Org,
            Name = properties.Name,
        };
}
