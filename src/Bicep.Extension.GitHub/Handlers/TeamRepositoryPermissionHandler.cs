using Bicep.Local.Extension.Host.Handlers;

namespace Bicep.Extension.Github.Handlers;

public class TeamRepositoryPermissionHandler : GithubResourceHandlerBase<TeamRepositoryPermission, TeamRepositoryPermissionIdentifiers>
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
            await client.Organization.Team.AddOrUpdateTeamRepositoryPermissions(
                request.Properties.Org,
                request.Properties.TeamSlug,
                request.Properties.Owner,
                request.Properties.Repo,
                request.Properties.Permission);

            return GetResponse(request);
        });

    protected override TeamRepositoryPermissionIdentifiers GetIdentifiers(TeamRepositoryPermission properties)
        => new()
        {
            Org = properties.Org,
            TeamSlug = properties.TeamSlug,
            Owner = properties.Owner,
            Repo = properties.Repo,
        };
}
