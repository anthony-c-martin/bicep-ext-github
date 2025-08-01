using System.Text.Json.Serialization;
using Azure.Bicep.Types.Concrete;
using Bicep.Local.Extension.Types.Attributes;

namespace Bicep.Extension.Github;

public enum Visibility
{
    Public,
    Private,
    Internal
}

public class RepositoryIdentifiers
{
    [TypeProperty("The owner of the repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string Owner { get; set; }

    [TypeProperty("The repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string Name { get; set; }
}

[ResourceType("Repository")]
public class Repository : RepositoryIdentifiers
{
    [TypeProperty("The repository description")]
    public string? Description { get; set; }

    [TypeProperty("The repository homepage url")]
    public string? Homepage { get; set; }

    [TypeProperty("The repository visibility")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Visibility? Visibility { get; set; }

    [TypeProperty("Whether the repository has issues enabled")]
    public bool HasIssues { get; set; }

    [TypeProperty("Whether the repository has projects enabled")]
    public bool HasProjects { get; set; }

    [TypeProperty("Whether the repository has wiki enabled")]
    public bool HasWiki { get; set; }

    [TypeProperty("Whether the repository has downloads enabled")]
    public bool HasDownloads { get; set; }

    [TypeProperty("Whether the repository is a template")]
    public bool IsTemplate { get; set; }

    [TypeProperty("Whether to allow squash merges")]
    public bool AllowSquashMerge { get; set; }

    [TypeProperty("Whether to allow merge commits")]
    public bool AllowMergeCommit { get; set; }

    [TypeProperty("Whether to allow rebase merges")]
    public bool AllowRebaseMerge { get; set; }

    [TypeProperty("Whether to automatically delete head branches")]
    public bool DeleteBranchOnMerge { get; set; }

    [TypeProperty("Whether to allow auto-merge")]
    public bool AllowAutoMerge { get; set; }
}

public class CollaboratorIdentifiers
{
    [TypeProperty("The owner of the repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string Owner { get; set; }

    [TypeProperty("The repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string Repo { get; set; }

    [TypeProperty("The collaborator user handle", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string User { get; set; }
}

[ResourceType("Collaborator")]
public class Collaborator : CollaboratorIdentifiers
{
    [TypeProperty("The collaborator permissions")]
    public string? Permission { get; set; }
}

public class LabelIdentifiers
{
    [TypeProperty("The owner of the repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string Owner { get; set; }

    [TypeProperty("The repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string Repo { get; set; }

    [TypeProperty("The label name", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string Name { get; set; }
}

[ResourceType("Label")]
public class Label : LabelIdentifiers
{
    [TypeProperty("The label color", ObjectTypePropertyFlags.Required)]
    public required string Color { get; set; }

    [TypeProperty("The label description", ObjectTypePropertyFlags.None)]
    public string? Description { get; set; }
}

public class ActionsSecretIdentifiers
{
    [TypeProperty("The owner of the repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string Owner { get; set; }

    [TypeProperty("The repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string Repo { get; set; }

    [TypeProperty("The secret name", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string Name { get; set; }
}

[ResourceType("ActionsSecret")]
public class ActionsSecret : ActionsSecretIdentifiers
{
    [TypeProperty("The secret value", ObjectTypePropertyFlags.Required | ObjectTypePropertyFlags.WriteOnly, isSecure: true)]
    public string? Value { get; set; }
}

public class ActionsVariableIdentifiers
{
    [TypeProperty("The owner of the repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string Owner { get; set; }

    [TypeProperty("The repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string Repo { get; set; }

    [TypeProperty("The variable name", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string Name { get; set; }
}

[ResourceType("ActionsVariable")]
public class ActionsVariable : ActionsVariableIdentifiers
{
    [TypeProperty("The variable value", ObjectTypePropertyFlags.Required, isSecure: false)]
    public required string Value { get; set; }
}

public class Configuration
{
    [TypeProperty("The GitHub personal access token with the required permissions", ObjectTypePropertyFlags.Required, isSecure: true)]
    public required string Token { get; set; }
}