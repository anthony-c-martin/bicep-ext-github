using Bicep.Local.Extension.Host.Handlers;
using Octokit;

namespace Bicep.Extension.Github.Handlers;

public class RepositoryWebhookHandler : GithubResourceHandlerBase<RepositoryWebhook, RepositoryWebhookIdentifiers>
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
            var hooks = await client.Repository.Hooks.GetAll(request.Properties.Owner, request.Properties.Repo);
            var existing = hooks.FirstOrDefault(hook =>
                hook.Name.Equals("web", StringComparison.OrdinalIgnoreCase) &&
                hook.Config.TryGetValue("url", out var existingUrl) &&
                string.Equals(existingUrl, request.Properties.Url, StringComparison.OrdinalIgnoreCase));

            var config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["url"] = request.Properties.Url,
                ["content_type"] = request.Properties.ContentType,
                ["insecure_ssl"] = request.Properties.InsecureSsl ? "1" : "0",
            };

            if (!string.IsNullOrWhiteSpace(request.Properties.Secret))
            {
                config["secret"] = request.Properties.Secret;
            }

            if (existing is null)
            {
                var create = new NewRepositoryHook("web", config)
                {
                    Active = request.Properties.Active,
                    Events = request.Properties.Events ?? ["push"],
                };

                await client.Repository.Hooks.Create(request.Properties.Owner, request.Properties.Repo, create);
            }
            else
            {
                var edit = new EditRepositoryHook(config)
                {
                    Active = request.Properties.Active,
                    Events = request.Properties.Events,
                };

                await client.Repository.Hooks.Edit(request.Properties.Owner, request.Properties.Repo, existing.Id, edit);
            }

            // never return secret content
            request.Properties.Secret = null;

            return GetResponse(request);
        });

    protected override RepositoryWebhookIdentifiers GetIdentifiers(RepositoryWebhook properties)
        => new()
        {
            Owner = properties.Owner,
            Repo = properties.Repo,
            Url = properties.Url,
        };
}
