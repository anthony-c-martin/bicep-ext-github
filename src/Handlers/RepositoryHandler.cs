using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Bicep.Local.Extension.Host.Handlers;
using Octokit;

namespace Bicep.Extension.Github.Handlers;

public class RepositoryHandler : GithubResourceHandlerBase<Repository, RepositoryIdentifiers>
{
    protected override Task<ResourceResponse> Preview(ResourceRequest request, CancellationToken cancellationToken)
        => HandleRequest(request, async client =>
        {
            await Task.Yield();

            return GetResponse(request);
        });

    protected override Task<ResourceResponse> CreateOrUpdate(ResourceRequest request, CancellationToken cancellationToken)
        => HandleRequest(request, async client =>
        {
            var user = await client.User.Current();

            try
            {
                await client.Repository.Edit(
                    request.Properties.Owner,
                    request.Properties.Name,
                    new RepositoryUpdate()
                    {
                        Description = request.Properties.Description,
                        Homepage = request.Properties.Homepage,
                        Visibility = request.Properties.Visibility switch
                        {
                            null => null,
                            Visibility.Public => RepositoryVisibility.Public,
                            Visibility.Private => RepositoryVisibility.Private,
                            Visibility.Internal => RepositoryVisibility.Internal,
                            _ => throw new NotImplementedException(),
                        },
                        HasIssues = request.Properties.HasIssues,
                        HasProjects = request.Properties.HasProjects,
                        HasWiki = request.Properties.HasWiki,
                        HasDownloads = request.Properties.HasDownloads,
                        IsTemplate = request.Properties.IsTemplate,
                        AllowSquashMerge = request.Properties.AllowSquashMerge,
                        AllowMergeCommit = request.Properties.AllowMergeCommit,
                        AllowRebaseMerge = request.Properties.AllowRebaseMerge,
                        DeleteBranchOnMerge = request.Properties.DeleteBranchOnMerge,
                        AllowAutoMerge = request.Properties.AllowAutoMerge,
                    });
            }
            catch (NotFoundException)
            {
                NewRepository repository = new(request.Properties.Name)
                {
                    Description = request.Properties.Description,
                    Homepage = request.Properties.Homepage,
                    Visibility = request.Properties.Visibility switch
                    {
                        null => null,
                        Visibility.Public => RepositoryVisibility.Public,
                        Visibility.Private => RepositoryVisibility.Private,
                        Visibility.Internal => RepositoryVisibility.Internal,
                        _ => throw new NotImplementedException(),
                    },
                    HasIssues = request.Properties.HasIssues,
                    HasProjects = request.Properties.HasProjects,
                    HasWiki = request.Properties.HasWiki,
                    HasDownloads = request.Properties.HasDownloads,
                    IsTemplate = request.Properties.IsTemplate,
                    AllowSquashMerge = request.Properties.AllowSquashMerge,
                    AllowMergeCommit = request.Properties.AllowMergeCommit,
                    AllowRebaseMerge = request.Properties.AllowRebaseMerge,
                    DeleteBranchOnMerge = request.Properties.DeleteBranchOnMerge,
                    AllowAutoMerge = request.Properties.AllowAutoMerge,
                };

                if (request.Properties.Owner == user.Login)
                {
                    await client.Repository.Create(repository);
                }
                else
                {
                    await client.Repository.Create(request.Properties.Owner, repository);
                }
            }

            return GetResponse(request);
        });

    protected override RepositoryIdentifiers GetIdentifiers(Repository properties)
        => new()
        {
            Owner = properties.Owner,
            Name = properties.Name,
        };
}