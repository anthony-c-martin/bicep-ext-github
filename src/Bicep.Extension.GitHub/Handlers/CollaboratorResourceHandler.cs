// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Text.Json;
using System.Text.Json.Nodes;
using Bicep.Local.Extension.Protocol;
using Octokit;

namespace Bicep.Extension.Github.Handlers;

public class CollaboratorResourceHandler : IResourceHandler
{
    public string ResourceType => "Collaborator";

    private record Identifiers(
        string? Owner,
        string? Repo,
        string? User);

    public Task<LocalExtensibilityOperationResponse> Delete(ResourceReference request, CancellationToken cancellationToken)
        => throw new NotImplementedException();

    public Task<LocalExtensibilityOperationResponse> Get(ResourceReference request, CancellationToken cancellationToken)
        => throw new NotImplementedException();

    public Task<LocalExtensibilityOperationResponse> Preview(ResourceSpecification request, CancellationToken cancellationToken)
        => RequestHelper.HandleRequest(request.Config, async client => {
            var properties = RequestHelper.GetProperties<Types.Github.Models.Collaborator>(request.Properties);
            
            await Task.Yield();

            return RequestHelper.CreateSuccessResponse(request, properties, new Identifiers(properties.Owner, properties.Repo, properties.User));
        });

    public Task<LocalExtensibilityOperationResponse> CreateOrUpdate(ResourceSpecification request, CancellationToken cancellationToken)
        => RequestHelper.HandleRequest(request.Config, async client => {
            var properties = RequestHelper.GetProperties<Types.Github.Models.Collaborator>(request.Properties);

            await client.Repository.Collaborator.Add(
                properties.Owner,
                properties.Repo,
                properties.User,
                new(properties.Permission));

            return RequestHelper.CreateSuccessResponse(request, properties, new Identifiers(properties.Owner, properties.Repo, properties.User));
        });
}