using System;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json.Serialization;
using Azure.Bicep.Types;
using Azure.Bicep.Types.Concrete;
using Azure.Bicep.Types.Index;
using Azure.Bicep.Types.Serialization;

namespace Bicep.Types.Github.Models;

[AttributeUsage(AttributeTargets.Property)]
public class TypeAnnotationAttribute : Attribute
{
    public TypeAnnotationAttribute(
        string? description,
        ObjectTypePropertyFlags flags = ObjectTypePropertyFlags.None,
        bool isSecure = false)
    {
        Description = description;
        Flags = flags;
        IsSecure = isSecure;
    }

    public string? Description { get; }

    public ObjectTypePropertyFlags Flags { get; }

    public bool IsSecure { get; }
}

public enum Visibility
{
    Public,
    Private,
    Internal
}
    
public class Repository
{
    [TypeAnnotation("The owner of the repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public string? Owner { get; set; }

    [TypeAnnotation("The repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public string? Name { get; set; }

    [TypeAnnotation("The repository description")]
    public string? Description { get; set; }

    [TypeAnnotation("The repository homepage url")]
    public string? Homepage { get; set; }

    [TypeAnnotation("The repository visibility")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Visibility? Visibility { get; set; }
}

public class Collaborator
{
    [TypeAnnotation("The owner of the repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public string? Owner { get; set; }

    [TypeAnnotation("The repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public string? Repo { get; set; }

    public string? User { get; set; }

    public string? Permission { get; set; }
}

public class Label
{
    [TypeAnnotation("The owner of the repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public string? Owner { get; set; }

    [TypeAnnotation("The repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public string? Repo { get; set; }

    [TypeAnnotation("The label name", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public string? Name { get; set; }

    public string? Color { get; set; }

    public string? Description { get; set; }
}

public class ActionsSecret
{
    [TypeAnnotation("The owner of the repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public string? Owner { get; set; }

    [TypeAnnotation("The repository", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public string? Repo { get; set; }

    [TypeAnnotation("The secret name", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public string? Name { get; set; }

    [TypeAnnotation("The secret value", ObjectTypePropertyFlags.Required, isSecure: true)]
    public string? Value { get; set; }
}