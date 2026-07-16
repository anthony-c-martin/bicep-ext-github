using System.Text.Json.Nodes;
using Bicep.Local.Extension.Host.Handlers;
using Octokit;

namespace Bicep.Extension.Github.Handlers;

public class LabelHandler : GithubResourceHandlerBase<Label, LabelIdentifiers>
{
    protected override Task<ResourceResponse> Preview(ResourceRequest request, CancellationToken cancellationToken)
        => HandleRequest(request, async client =>
        {
            await Task.Yield();

            return GetResponse(request);
        });

    protected override Task<ResourceResponse> CreateOrUpdate(ResourceRequest request, CancellationToken cancellationToken)
        => HandleRequest(request, async client => {
            try {
                await client.Issue.Labels.Get(
                    request.Properties.Owner,
                    request.Properties.Repo,
                    request.Properties.Name);

                await client.Issue.Labels.Update(
                    request.Properties.Owner,
                    request.Properties.Repo,
                    request.Properties.Name,
                    new(request.Properties.Name, request.Properties.Color)
                    {
                        Color = request.Properties.Color,
                        Description = request.Properties.Description,
                    });
            } catch (NotFoundException) {
                await client.Issue.Labels.Create(
                request.Properties.Owner,
                request.Properties.Repo,
                new(request.Properties.Name, request.Properties.Color)
                {
                    Color = request.Properties.Color,
                    Description = request.Properties.Description,
                });
            }

            return GetResponse(request);
        });

    protected override LabelIdentifiers GetIdentifiers(Label properties)
        => new()
        {
            Owner = properties.Owner,
            Repo = properties.Repo,
            Name = properties.Name,
        };
}