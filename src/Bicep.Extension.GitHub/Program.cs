using Microsoft.AspNetCore.Builder;
using Bicep.Local.Extension.Host.Extensions;
using Bicep.Extension.Github.Handlers;
using Azure.Bicep.Types.Concrete;
using Microsoft.Extensions.DependencyInjection;
using Bicep.Extension.Github;

var builder = WebApplication.CreateBuilder();

builder.AddBicepExtensionHost(args);
builder.Services
    .AddBicepExtension()
    .WithDefaults(
        name: ThisAssembly.AssemblyName.Split('-')[^1],
        version: ThisAssembly.AssemblyInformationalVersion.Split('+')[0],
        isSingleton: true)
    .WithTypeAssembly(typeof(Program).Assembly)
    .WithConfigurationType(typeof(Configuration))
    .WithResourceHandler<RepositoryHandler>()
    .WithResourceHandler<CollaboratorHandler>()
    .WithResourceHandler<LabelHandler>()
    .WithResourceHandler<ActionsSecretHandler>()
    .WithResourceHandler<ActionsVariableHandler>()
    .WithResourceHandler<OrganizationActionsSecretHandler>()
    .WithResourceHandler<OrganizationActionsVariableHandler>()
    .WithResourceHandler<DependabotSecretHandler>()
    .WithResourceHandler<BranchProtectionRuleHandler>()
    .WithResourceHandler<RepositoryRulesetHandler>()
    .WithResourceHandler<TeamRepositoryPermissionHandler>()
    .WithResourceHandler<TeamHandler>()
    .WithResourceHandler<TeamMembershipHandler>()
    .WithResourceHandler<EnvironmentHandler>()
    .WithResourceHandler<EnvironmentSecretHandler>()
    .WithResourceHandler<EnvironmentVariableHandler>()
    .WithResourceHandler<RepositoryActionsPermissionsHandler>()
    .WithResourceHandler<BranchHandler>()
    .WithResourceHandler<RepositoryWebhookHandler>()
    .WithResourceHandler<DeployKeyHandler>()
    .WithResourceHandler<GitHubFileHandler>();

var app = builder.Build();
app.MapBicepExtension();

await app.RunAsync();