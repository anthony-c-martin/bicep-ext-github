using Bicep.Local.Extension.Host.Handlers;
using Octokit;

namespace Bicep.Extension.Github.Handlers;

public class DeployKeyHandler : GithubResourceHandlerBase<DeployKey, DeployKeyIdentifiers>
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
            var keys = await client.Repository.DeployKeys.GetAll(request.Properties.Owner, request.Properties.Repo);
            var existing = keys.FirstOrDefault(key =>
                string.Equals(key.Key, request.Properties.Key, StringComparison.Ordinal) ||
                string.Equals(key.Title, request.Properties.Title, StringComparison.Ordinal));

            if (existing is null)
            {
                var create = new NewDeployKey
                {
                    Title = request.Properties.Title,
                    Key = request.Properties.Key,
                    ReadOnly = request.Properties.ReadOnly,
                };

                await client.Repository.DeployKeys.Create(request.Properties.Owner, request.Properties.Repo, create);
            }
            else if (!string.Equals(existing.Key, request.Properties.Key, StringComparison.Ordinal) ||
                     !string.Equals(existing.Title, request.Properties.Title, StringComparison.Ordinal))
            {
                await client.Repository.DeployKeys.Delete(request.Properties.Owner, request.Properties.Repo, existing.Id);

                var recreate = new NewDeployKey
                {
                    Title = request.Properties.Title,
                    Key = request.Properties.Key,
                    ReadOnly = request.Properties.ReadOnly,
                };

                await client.Repository.DeployKeys.Create(request.Properties.Owner, request.Properties.Repo, recreate);
            }

            return GetResponse(request);
        });

    protected override DeployKeyIdentifiers GetIdentifiers(DeployKey properties)
        => new()
        {
            Owner = properties.Owner,
            Repo = properties.Repo,
            Title = properties.Title,
        };
}
