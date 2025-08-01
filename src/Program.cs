using Microsoft.AspNetCore.Builder;
using Bicep.Local.Extension.Host.Extensions;
using Bicep.Extension.Github.Handlers;
using Azure.Bicep.Types.Concrete;
using Microsoft.Extensions.DependencyInjection;
using Bicep.Extension.Github;

var builder = WebApplication.CreateBuilder();

builder.AddBicepExtensionHost(args);
builder.Services
    .AddBicepExtension(
        name: ThisAssembly.AssemblyName.Split('-')[^1],
        version: ThisAssembly.AssemblyInformationalVersion.Split('+')[0],
        isSingleton: true,
        typeAssembly: typeof(Program).Assembly,
        configurationType: typeof(Configuration))
    .WithResourceHandler<RepositoryHandler>()
    .WithResourceHandler<CollaboratorHandler>()
    .WithResourceHandler<LabelHandler>()
    .WithResourceHandler<ActionsSecretHandler>()
    .WithResourceHandler<ActionsVariableHandler>();

var app = builder.Build();
app.MapBicepExtension();

await app.RunAsync();