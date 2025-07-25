using System.Text.Json.Nodes;
using Bicep.Local.Extension.Host.Handlers;
using Octokit;

namespace Bicep.Extension.Github.Handlers;

public class ActionsVariableHandler : GithubResourceHandlerBase<ActionsVariable, ActionsVariableIdentifiers>
{
    protected override Task<ResourceResponse> Preview(ResourceRequest request, CancellationToken cancellationToken)
        => HandleRequest(request, async client =>
        {
            await Task.CompletedTask;

            return GetResponse(request);
        });

    protected override Task<ResourceResponse> CreateOrUpdate(ResourceRequest request, CancellationToken cancellationToken)
        => HandleRequest(request, async client =>
        {
            try
            {
                await client.Repository.Actions.Variables.Create(
                    request.Properties.Owner,
                    request.Properties.Repo,
                    new(request.Properties.Name, request.Properties.Value));
            }
            catch (ApiException ex) when (ex is { StatusCode: System.Net.HttpStatusCode.Conflict })
            {
                await client.Repository.Actions.Variables.Update(
                    request.Properties.Owner,
                    request.Properties.Repo,
                    new(request.Properties.Name, request.Properties.Value));
            }

            return GetResponse(request);
        });

    protected override ActionsVariableIdentifiers GetIdentifiers(ActionsVariable properties)
        => new()
        {
            Owner = properties.Owner,
            Repo = properties.Repo,
            Name = properties.Name,
        };
}
