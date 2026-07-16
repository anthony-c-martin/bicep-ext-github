using Bicep.Local.Extension.Host.Handlers;
using Octokit;

namespace Bicep.Extension.Github.Handlers;

public class TeamMembershipHandler : GithubResourceHandlerBase<TeamMembership, TeamMembershipIdentifiers>
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
            var role = ParseRole(request.Properties.Role);

            var team = await client.Organization.Team.GetByName(
                request.Properties.Org,
                request.Properties.TeamSlug);

            await client.Organization.Team.AddOrEditMembership(
                team.Id,
                request.Properties.Username,
                new UpdateTeamMembership(role));

            return GetResponse(request);
        });

    protected override TeamMembershipIdentifiers GetIdentifiers(TeamMembership properties)
        => new()
        {
            Org = properties.Org,
            TeamSlug = properties.TeamSlug,
            Username = properties.Username,
        };

    private static TeamRole ParseRole(string role)
        => role.ToLowerInvariant() switch
        {
            "member" => TeamRole.Member,
            "maintainer" => TeamRole.Maintainer,
            _ => throw new ResourceErrorException(
                "InvalidRole",
                $"Invalid team role '{role}'. Expected 'member' or 'maintainer'."),
        };
}
