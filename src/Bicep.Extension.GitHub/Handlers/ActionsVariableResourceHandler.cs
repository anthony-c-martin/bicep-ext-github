// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Text.Json;
using System.Text.Json.Nodes;
using Bicep.Local.Extension.Protocol;
using Octokit;
using Sodium;

namespace Bicep.Extension.Github.Handlers;

public class ActionsVariableResourceHandler : IResourceHandler
{
    public string ResourceType => "ActionsVariable";

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
            var properties = RequestHelper.GetProperties<Types.Github.Models.ActionsVariable>(request.Properties);

            await Task.Yield();

            return RequestHelper.CreateSuccessResponse(request, properties, new Identifiers(properties.Owner, properties.Repo, properties.Name));
        });

    public Task<LocalExtensibilityOperationResponse> CreateOrUpdate(ResourceSpecification request, CancellationToken cancellationToken)
        => RequestHelper.HandleRequest(request.Config, async client => {
            var properties = RequestHelper.GetProperties<Types.Github.Models.ActionsVariable>(request.Properties);

            try
            {
                await client.Repository.Actions.Variables.Create(
                    properties.Owner,
                    properties.Repo,
                    new(properties.Name, properties.Value));
            }
            catch (ApiException ex) when (ex is { StatusCode: System.Net.HttpStatusCode.Conflict })
            {
                await client.Repository.Actions.Variables.Update(
                    properties.Owner,
                    properties.Repo,
                    new(properties.Name, properties.Value));
            }

            return RequestHelper.CreateSuccessResponse(request, properties, new Identifiers(properties.Owner, properties.Repo, properties.Name));
        });
}
