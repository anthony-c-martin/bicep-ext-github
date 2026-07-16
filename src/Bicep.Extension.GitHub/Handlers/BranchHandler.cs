using Bicep.Local.Extension.Host.Handlers;
using Octokit;

namespace Bicep.Extension.Github.Handlers;

public class BranchHandler : GithubResourceHandlerBase<Branch, BranchIdentifiers>
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
            try
            {
                var existing = await client.Git.Reference.Get(
                    request.Properties.Owner,
                    request.Properties.Repo,
                    $"heads/{request.Properties.Name}");

                request.Properties.Sha = existing.Object.Sha;
                return GetResponse(request);
            }
            catch (NotFoundException)
            {
                // branch does not exist yet, create it below
            }

            var sha = request.Properties.SourceSha;
            if (string.IsNullOrWhiteSpace(sha))
            {
                var sourceBranch = request.Properties.SourceBranch;
                if (string.IsNullOrWhiteSpace(sourceBranch))
                {
                    var repository = await client.Repository.Get(request.Properties.Owner, request.Properties.Repo);
                    sourceBranch = repository.DefaultBranch;
                }

                var sourceReference = await client.Git.Reference.Get(
                    request.Properties.Owner,
                    request.Properties.Repo,
                    $"heads/{sourceBranch}");

                sha = sourceReference.Object.Sha;
            }

            var created = await client.Git.Reference.Create(
                request.Properties.Owner,
                request.Properties.Repo,
                new NewReference($"refs/heads/{request.Properties.Name}", sha));

            request.Properties.Sha = created.Object.Sha;

            return GetResponse(request);
        });

    protected override BranchIdentifiers GetIdentifiers(Branch properties)
        => new()
        {
            Owner = properties.Owner,
            Repo = properties.Repo,
            Name = properties.Name,
        };
}
