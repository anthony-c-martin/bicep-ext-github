// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Bicep.Local.Extension.Protocol;
using Octokit;

namespace Bicep.Extension.Github.Handlers;

public class LabelResourceHandler : IResourceHandler
{
    public string ResourceType => "Label";

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
            var properties = RequestHelper.GetProperties<Types.Github.Models.Label>(request.Properties);
            
            await Task.Yield();

            return RequestHelper.CreateSuccessResponse(request, properties, new Identifiers(properties.Owner, properties.Repo, properties.Name));
        });

    public Task<LocalExtensibilityOperationResponse> CreateOrUpdate(ResourceSpecification request, CancellationToken cancellationToken)
        => RequestHelper.HandleRequest(request.Config, async client => {
            var properties = RequestHelper.GetProperties<Types.Github.Models.Label>(request.Properties);

            try {
                await client.Issue.Labels.Get(
                    properties.Owner,
                    properties.Repo,
                    properties.Name);

                await client.Issue.Labels.Update(
                    properties.Owner,
                    properties.Repo,
                    properties.Name,
                    new(properties.Name, properties.Color)
                    {
                        Color = properties.Color,
                        Description = properties.Description,
                    });
            } catch (NotFoundException) {
                await client.Issue.Labels.Create(
                properties.Owner,
                properties.Repo,
                new(properties.Name, properties.Color)
                {
                    Color = properties.Color,
                    Description = properties.Description,
                });
            }

            return RequestHelper.CreateSuccessResponse(request, properties, new Identifiers(properties.Owner, properties.Repo, properties.Name));
        });
}