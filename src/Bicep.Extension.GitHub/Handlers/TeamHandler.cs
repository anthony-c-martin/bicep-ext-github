using Bicep.Local.Extension.Host.Handlers;
using Octokit;

namespace Bicep.Extension.Github.Handlers;

public class TeamHandler : GithubResourceHandlerBase<Team, TeamIdentifiers>
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
            var privacy = ParsePrivacy(request.Properties.Privacy);

            Octokit.Team? existingTeam = null;
            try
            {
                existingTeam = await client.Organization.Team.GetByName(
                    request.Properties.Org,
                    Slugify(request.Properties.Name));
            }
            catch (NotFoundException)
            {
                existingTeam = null;
            }

            Octokit.Team result;
            if (existingTeam is null)
            {
                var newTeam = new NewTeam(request.Properties.Name)
                {
                    Description = request.Properties.Description,
                    Privacy = privacy,
                };

                if (request.Properties.ParentTeamId is { } parentId)
                {
                    newTeam.ParentTeamId = parentId;
                }

                result = await client.Organization.Team.Create(request.Properties.Org, newTeam);
            }
            else
            {
                var updateTeam = new UpdateTeam(request.Properties.Name)
                {
                    Description = request.Properties.Description,
                    Privacy = privacy,
                };

                if (request.Properties.ParentTeamId is { } parentId)
                {
                    updateTeam.ParentTeamId = parentId;
                }

                result = await client.Organization.Team.Update(
                    request.Properties.Org,
                    existingTeam.Slug,
                    updateTeam);
            }

            request.Properties.Slug = result.Slug;
            request.Properties.Id = (int)result.Id;

            return GetResponse(request);
        });

    protected override TeamIdentifiers GetIdentifiers(Team properties)
        => new()
        {
            Org = properties.Org,
            Name = properties.Name,
        };

    private static TeamPrivacy ParsePrivacy(string privacy)
        => privacy.ToLowerInvariant() switch
        {
            "secret" => TeamPrivacy.Secret,
            "closed" => TeamPrivacy.Closed,
            _ => throw new ResourceErrorException(
                "InvalidPrivacy",
                $"Invalid team privacy '{privacy}'. Expected 'secret' or 'closed'."),
        };

    private static string Slugify(string name)
    {
        var chars = name
            .Trim()
            .ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray();

        var slug = new string(chars);
        while (slug.Contains("--"))
        {
            slug = slug.Replace("--", "-");
        }

        return slug.Trim('-');
    }
}
