using Bicep.Local.Extension.Host.Handlers;
using Octokit;

namespace Bicep.Extension.Github.Handlers;

public class GitHubFileHandler : GithubResourceHandlerBase<GitHubFile, GitHubFileIdentifiers>
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
            RepositoryContent? existingFile = null;

            try
            {
                IReadOnlyList<RepositoryContent> existingContents = string.IsNullOrWhiteSpace(request.Properties.Branch)
                    ? await client.Repository.Content.GetAllContents(request.Properties.Owner, request.Properties.Repo, request.Properties.Path)
                    : await client.Repository.Content.GetAllContentsByRef(request.Properties.Owner, request.Properties.Repo, request.Properties.Path, request.Properties.Branch);

                existingFile = existingContents.FirstOrDefault(content => content.Type == ContentType.File);
            }
            catch (NotFoundException)
            {
                existingFile = null;
            }

            var commitMessage = string.IsNullOrWhiteSpace(request.Properties.CommitMessage)
                ? $"Manage {request.Properties.Path} via bicep-ext-github"
                : request.Properties.CommitMessage;

            RepositoryContentChangeSet changeSet;
            if (existingFile is null)
            {
                var createRequest = string.IsNullOrWhiteSpace(request.Properties.Branch)
                    ? new CreateFileRequest(commitMessage, request.Properties.Content)
                    : new CreateFileRequest(commitMessage, request.Properties.Content, request.Properties.Branch);

                changeSet = await client.Repository.Content.CreateFile(request.Properties.Owner, request.Properties.Repo, request.Properties.Path, createRequest);
            }
            else if (string.Equals(existingFile.Content, request.Properties.Content, StringComparison.Ordinal))
            {
                // The file already has the desired content, so avoid creating an
                // empty commit and just surface the existing file's metadata.
                request.Properties.Sha = existingFile.Sha;
                request.Properties.HtmlUrl = existingFile.HtmlUrl;
                request.Properties.DownloadUrl = existingFile.DownloadUrl;

                return GetResponse(request);
            }
            else
            {
                var updateRequest = string.IsNullOrWhiteSpace(request.Properties.Branch)
                    ? new UpdateFileRequest(commitMessage, request.Properties.Content, existingFile.Sha)
                    : new UpdateFileRequest(commitMessage, request.Properties.Content, existingFile.Sha, request.Properties.Branch);

                changeSet = await client.Repository.Content.UpdateFile(request.Properties.Owner, request.Properties.Repo, request.Properties.Path, updateRequest);
            }

            request.Properties.Sha = changeSet.Content.Sha;
            request.Properties.HtmlUrl = changeSet.Content.HtmlUrl;
            request.Properties.DownloadUrl = changeSet.Content.DownloadUrl;

            return GetResponse(request);
        });

    protected override GitHubFileIdentifiers GetIdentifiers(GitHubFile properties)
        => new()
        {
            Owner = properties.Owner,
            Repo = properties.Repo,
            Path = properties.Path,
        };
}
