using System.Text.Json.Nodes;
using Bicep.Local.Extension.Host.Handlers;

namespace Bicep.Extension.Github.Handlers;

public class CollaboratorHandler : GithubResourceHandlerBase<Collaborator, CollaboratorIdentifiers>
{
    protected override Task<ResourceResponse> Preview(ResourceRequest request, CancellationToken cancellationToken)
        => HandleRequest(request, async client =>
        {
            await Task.Yield();

            return GetResponse(request);
        });

    protected override Task<ResourceResponse> CreateOrUpdate(ResourceRequest request, CancellationToken cancellationToken)
        => HandleRequest(request, async client => {
            await client.Repository.Collaborator.Add(
                request.Properties.Owner,
                request.Properties.Repo,
                request.Properties.User,
                new(request.Properties.Permission));

            return GetResponse(request);
        });

    protected override CollaboratorIdentifiers GetIdentifiers(Collaborator properties)
        => new()
        {
            Owner = properties.Owner,
            Repo = properties.Repo,
            User = properties.User,
        };
}