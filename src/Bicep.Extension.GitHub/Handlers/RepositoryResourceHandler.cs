// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Bicep.Local.Extension.Protocol;
using Octokit;

namespace Bicep.Extension.Github.Handlers;

public class RepositoryResourceHandler : IResourceHandler
{
    public string ResourceType => "Repository";

    private record Identifiers(
        string? Owner,
        string? Name);

    public Task<LocalExtensibilityOperationResponse> Delete(ResourceReference request, CancellationToken cancellationToken)
        => throw new NotImplementedException();

    public Task<LocalExtensibilityOperationResponse> Get(ResourceReference request, CancellationToken cancellationToken)
        => throw new NotImplementedException();

    public Task<LocalExtensibilityOperationResponse> Preview(ResourceSpecification request, CancellationToken cancellationToken)
        => RequestHelper.HandleRequest(request.Config, async client =>
        {
            var properties = RequestHelper.GetProperties<Types.Github.Models.Repository>(request.Properties);

            await Task.Yield();

            return RequestHelper.CreateSuccessResponse(request, properties, new Identifiers(properties.Owner, properties.Name));
        });

    public Task<LocalExtensibilityOperationResponse> CreateOrUpdate(ResourceSpecification request, CancellationToken cancellationToken)
        => RequestHelper.HandleRequest(request.Config, async client =>
        {
            var properties = RequestHelper.GetProperties<Types.Github.Models.Repository>(request.Properties);
            var user = await client.User.Current();

            try
            {
                await client.Repository.Edit(
                    properties.Owner,
                    properties.Name,
                    new()
                    {
                        Description = properties.Description,
                        Homepage = properties.Homepage,
                        Visibility = properties.Visibility switch
                        {
                            null => null,
                            Types.Github.Models.Visibility.Public => RepositoryVisibility.Public,
                            Types.Github.Models.Visibility.Private => RepositoryVisibility.Private,
                            Types.Github.Models.Visibility.Internal => RepositoryVisibility.Internal,
                            _ => throw new NotImplementedException(),
                        }
                    });
            }
            catch (NotFoundException)
            {
                if (properties.Owner == user.Login)
                {
                    await client.Repository.Create(
                        new(properties.Name)
                        {
                            Description = properties.Description,
                            Homepage = properties.Homepage,
                            Visibility = properties.Visibility switch
                            {
                                null => null,
                                Types.Github.Models.Visibility.Public => RepositoryVisibility.Public,
                                Types.Github.Models.Visibility.Private => RepositoryVisibility.Private,
                                Types.Github.Models.Visibility.Internal => RepositoryVisibility.Internal,
                                _ => throw new NotImplementedException(),
                            }
                        });
                }
                else
                {
                    await client.Repository.Create(
                        properties.Owner,
                        new(properties.Name)
                        {
                            Description = properties.Description,
                            Homepage = properties.Homepage,
                            Visibility = properties.Visibility switch
                            {
                                null => null,
                                Types.Github.Models.Visibility.Public => RepositoryVisibility.Public,
                                Types.Github.Models.Visibility.Private => RepositoryVisibility.Private,
                                Types.Github.Models.Visibility.Internal => RepositoryVisibility.Internal,
                                _ => throw new NotImplementedException(),
                            }
                        });
                }
            }

            return RequestHelper.CreateSuccessResponse(request, properties, new Identifiers(properties.Owner, properties.Name));
        });
}