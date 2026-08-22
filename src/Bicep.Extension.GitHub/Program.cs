using Microsoft.AspNetCore.Builder;
using Bicep.Local.Extension.Host.Extensions;
using Bicep.Extension.Github.Handlers;
using Azure.Bicep.Types.Concrete;
using Microsoft.Extensions.DependencyInjection;
using Bicep.Extension.Github;
using System.Reflection;

var assembly = typeof(Program).Assembly;
var assemblyName = assembly.GetName().Name ?? "bicep-ext-github";
var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
    ?? assembly.GetName().Version?.ToString()
    ?? "0.0.0";

var builder = WebApplication.CreateBuilder();

builder.AddBicepExtensionHost(args);
builder.Services
    .AddBicepExtension()
    .WithDefaults(
        name: assemblyName.Split('-')[^1],
        version: informationalVersion.Split('+')[0],
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