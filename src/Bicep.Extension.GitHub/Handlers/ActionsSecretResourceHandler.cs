// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Text.Json;
using System.Text.Json.Nodes;
using Bicep.Local.Extension.Protocol;
using Octokit;
using Sodium;

namespace Bicep.Extension.Github.Handlers;

public class ActionsSecretResourceHandler : IResourceHandler
{
    public string ResourceType => "ActionsSecret";

    private record Identifiers(
        string? Owner,
        string? Repo,
        string? Name);

    public Task<LocalExtensibilityOperationResponse> Delete(ResourceReference request, CancellationToken cancellationToken)
        => throw new NotImplementedException();

    public Task<LocalExtensibilityOperationResponse> Get(ResourceReference request, CancellationToken cancellationToken)
        => throw new NotImplementedException();

    public Task<LocalExtensibilityOperationResponse> Preview(ResourceSpecification request, CancellationToken cancellationToken)
        => RequestHelper.HandleRequest(request.Config, async client => {
            var properties = RequestHelper.GetProperties<Types.Github.Models.ActionsSecret>(request.Properties);
            
            await Task.Yield();

            // don't return the value to Bicep
            properties.Value = null;

            return RequestHelper.CreateSuccessResponse(request, properties, new Identifiers(properties.Owner, properties.Repo, properties.Name));
        });

    public Task<LocalExtensibilityOperationResponse> CreateOrUpdate(ResourceSpecification request, CancellationToken cancellationToken)
        => RequestHelper.HandleRequest(request.Config, async client => {
            var properties = RequestHelper.GetProperties<Types.Github.Models.ActionsSecret>(request.Properties);

            var publicKey = await client.Repository.Actions.Secrets.GetPublicKey(properties.Owner, properties.Repo);

            var publicKeyEncoded = Convert.FromBase64String(publicKey.Key);
            var sealedPublicKeyBox = SealedPublicKeyBox.Create(properties.Value!, publicKeyEncoded);
            var encryptedSecret = Convert.ToBase64String(sealedPublicKeyBox);
            
            await client.Repository.Actions.Secrets.CreateOrUpdate(
                properties.Owner,
                properties.Repo,
                properties.Name,
                new()
                {
                    EncryptedValue = encryptedSecret,
                    KeyId = publicKey.KeyId,
                });

            // don't return the value to Bicep
            properties.Value = null;

            return RequestHelper.CreateSuccessResponse(request, properties, new Identifiers(properties.Owner, properties.Repo, properties.Name));
        });
}