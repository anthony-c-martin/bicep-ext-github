// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using Azure.Bicep.Types;
using Azure.Bicep.Types.Concrete;
using Azure.Bicep.Types.Index;
using Azure.Bicep.Types.Serialization;
using Bicep.Types.Github.Models;

namespace Bicep.Types.Github;

public static class TypeGenerator
{
    public static string CamelCase(string input)
        => $"{input[..1].ToLowerInvariant()}{input[1..]}";

    public static TypeBase GenerateForRecord(TypeFactory factory, ConcurrentDictionary<Type, TypeBase> typeCache, Type type)
    {
        var typeProperties = new Dictionary<string, ObjectTypeProperty>();
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var annotation = property.GetCustomAttributes<TypeAnnotationAttribute>(true).FirstOrDefault();
            var propertyType = property.PropertyType;
            TypeBase typeReference;

            if (propertyType == typeof(string) && annotation?.IsSecure == true)
            {
                typeReference = factory.Create(() => new StringType(sensitive: true));
            }
            else if (propertyType == typeof(string))
            {
                typeReference = typeCache.GetOrAdd(propertyType, _ => factory.Create(() => new StringType()));
            }
            else if (propertyType == typeof(bool))
            {
                typeReference = typeCache.GetOrAdd(propertyType, _ => factory.Create(() => new BooleanType()));
            }
            else if (propertyType == typeof(int))
            {
                typeReference = typeCache.GetOrAdd(propertyType, _ => factory.Create(() => new IntegerType()));
            }
            else if (propertyType.IsClass)
            {
                typeReference = typeCache.GetOrAdd(propertyType, _ => factory.Create(() => GenerateForRecord(factory, typeCache, propertyType)));
            }
            else if (propertyType.IsGenericType &&
                propertyType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                propertyType.GetGenericArguments()[0] is { IsEnum: true } enumType)
            {
                var enumMembers = enumType.GetEnumNames()
                    .Select(x => factory.Create(() => new StringLiteralType(x)))
                    .Select(x => factory.GetReference(x))
                    .ToImmutableArray();
                
                typeReference = typeCache.GetOrAdd(propertyType, _ => factory.Create(() => new UnionType(enumMembers)));
            }
            else
            {
                throw new NotImplementedException($"Unsupported property type {propertyType}");
            }

            typeProperties[CamelCase(property.Name)] = new ObjectTypeProperty(
                factory.GetReference(typeReference),
                annotation?.Flags ?? ObjectTypePropertyFlags.None,
                annotation?.Description);
        }

        return new ObjectType(
            type.Name,
            typeProperties,
            null);
    }

    public static ResourceType GenerateResource(TypeFactory factory, ConcurrentDictionary<Type, TypeBase> typeCache, Type type)
    {
        return factory.Create(() => new ResourceType(
            type.Name,
            ScopeType.Unknown,
            null,
            factory.GetReference(factory.Create(() => GenerateForRecord(factory, typeCache, type))),
            ResourceFlags.None,
            null));
    }

    public static string GetString(Action<Stream> streamWriteFunc)
    {
        using var memoryStream = new MemoryStream();
        streamWriteFunc(memoryStream);

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public static Dictionary<string, string> GenerateTypes()
    {
        var factory = new TypeFactory([]);
        var secureStringType = factory.Create(() => new StringType(sensitive: true));

        var configurationType = factory.Create(() => new ObjectType("configuration", new Dictionary<string, ObjectTypeProperty>
        {
            ["token"] = new(factory.GetReference(secureStringType), ObjectTypePropertyFlags.Required, null),
        }, null));

        var settings = new TypeSettings(
            name: "Github",
            version: "0.0.1",
            isSingleton: true,
            configurationType: new CrossFileTypeReference("types.json", factory.GetIndex(configurationType)));

        var typeCache = new ConcurrentDictionary<Type, TypeBase>();
        var resourceTypes = new[] {
            GenerateResource(factory, typeCache, typeof(Repository)),
            GenerateResource(factory, typeCache, typeof(Collaborator)),
            GenerateResource(factory, typeCache, typeof(Label)),
            GenerateResource(factory, typeCache, typeof(ActionsSecret)),
        };

        var index = new TypeIndex(
            resourceTypes.ToDictionary(x => x.Name, x => new CrossFileTypeReference("types.json", factory.GetIndex(x))),
            new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyList<CrossFileTypeReference>>>(),
            settings,
            null);

        return new Dictionary<string, string>{
            ["index.json"] = GetString(stream => TypeSerializer.SerializeIndex(stream, index)),
            ["types.json"] = GetString(stream => TypeSerializer.Serialize(stream, factory.GetTypes())),
        };
    }
}