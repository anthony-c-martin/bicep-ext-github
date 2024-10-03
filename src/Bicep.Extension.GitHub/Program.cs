// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Bicep.Extension.Github.Handlers;
using Bicep.Local.Extension;
using Bicep.Local.Extension.Protocol;
using Bicep.Local.Extension.Rpc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Bicep.Extension.Github;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var extension = new KestrelProviderExtension();

        await ProviderExtension.Run(new KestrelProviderExtension(), RegisterHandlers, args);
    }

    public static void RegisterHandlers(ResourceDispatcherBuilder builder) => builder
        .AddHandler(new CollaboratorResourceHandler())
        .AddHandler(new RepositoryResourceHandler());
}

public class KestrelProviderExtension : ProviderExtension
{
    protected override async Task RunServer(ConnectionOptions connectionOptions, ResourceDispatcher dispatcher, CancellationToken cancellationToken)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.ConfigureKestrel(options =>
        {
            switch (connectionOptions)
            {
                case { Socket: {}, Pipe: null }:
                    options.ListenUnixSocket(connectionOptions.Socket, listenOptions => listenOptions.Protocols = HttpProtocols.Http2);
                    break;
                case { Socket: null, Pipe: {} }:
                    options.ListenNamedPipe(connectionOptions.Pipe, listenOptions => listenOptions.Protocols = HttpProtocols.Http2);
                    break;
                default:
                    throw new InvalidOperationException("Either socketPath or pipeName must be specified.");
            }
        });

        builder.Services.AddGrpc();
        builder.Services.AddSingleton(dispatcher);
        var app = builder.Build();
        app.MapGrpcService<BicepExtensionImpl>();

        await app.RunAsync();
    }
}