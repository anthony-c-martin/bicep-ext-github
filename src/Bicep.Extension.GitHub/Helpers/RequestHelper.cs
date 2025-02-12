// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Bicep.Local.Extension.Protocol;
using Octokit;
using Octokit.Internal;

namespace Bicep.Extension.Github.Handlers;

public static class RequestHelper
{
    public static async Task<LocalExtensibilityOperationResponse> HandleRequest(JsonObject? config, Func<GitHubClient, Task<LocalExtensibilityOperationResponse>> onExecuteFunc)
    {
        var credentials = new Credentials(config!["token"]!.GetValue<string>());
        var client = new GitHubClient(new ProductHeaderValue("Bicep.LocalDeploy"), new InMemoryCredentialStore(credentials));

        try
        {
            return await onExecuteFunc(client);
        }
        catch (Exception exception)
        {
            if (exception is ApiException apiException)
            {
                var errorDetails = apiException.ApiError.Errors
                    .Select(error => new ErrorDetail(error.Code, error.Field ?? "", error.Message)).ToArray();

                return CreateErrorResponse("ApiError", apiException.ApiError.Message, errorDetails);
            }

            return CreateErrorResponse("UnhandledError", exception.Message);
        }
    }

    public static TProperties GetProperties<TProperties>(JsonObject properties)
        => properties.Deserialize<TProperties>(new JsonSerializerOptions(JsonSerializerDefaults.Web))!;

    public static LocalExtensibilityOperationResponse CreateSuccessResponse<TProperties, TIdentifiers>(ResourceReference request, TProperties properties, TIdentifiers identifiers)
    {
        return new(
            new(
                request.Type,
                request.ApiVersion,
                "Succeeded",
                (JsonNode.Parse(JsonSerializer.Serialize(identifiers, new JsonSerializerOptions(JsonSerializerDefaults.Web))) as JsonObject)!,
                request.Config,
                (JsonNode.Parse(JsonSerializer.Serialize(properties, new JsonSerializerOptions(JsonSerializerDefaults.Web))) as JsonObject)!),
            null);
    }

    public static LocalExtensibilityOperationResponse CreateSuccessResponse<TProperties, TIdentifiers>(ResourceSpecification request, TProperties properties, TIdentifiers identifiers)
    {
        return new(
            new(
                request.Type,
                request.ApiVersion,
                "Succeeded",
                (JsonNode.Parse(JsonSerializer.Serialize(identifiers, new JsonSerializerOptions(JsonSerializerDefaults.Web))) as JsonObject)!,
                request.Config,
                (JsonNode.Parse(JsonSerializer.Serialize(properties, new JsonSerializerOptions(JsonSerializerDefaults.Web))) as JsonObject)!),
            null);
    }

    public static LocalExtensibilityOperationResponse CreateErrorResponse(string code, string message, ErrorDetail[]? details = null, string? target = null)
    {
        return new LocalExtensibilityOperationResponse(
            null,
            new(new(code, target ?? "", message, details ?? [], [])));
    }
}
