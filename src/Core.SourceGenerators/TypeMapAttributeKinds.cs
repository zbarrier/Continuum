using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace Continuum.Core.SourceGenerators;

/// <summary>
/// The Continuum type map attributes and the <c>TypeMapKinds</c> value each declares, shared by the generator and
/// analyzers so they always agree on which attributes exist.
/// </summary>
internal static class TypeMapAttributeKinds
{
    public const string TypeMappingNamespace = "Continuum.TypeMapping";

    public const int Command = 1;
    public const int DomainEvent = 2;
    public const int IntegrationEvent = 4;
    public const int Snapshot = 8;
    public const int Metadata = 16;
    public const int Framework = 32;

    public const int All = Command | DomainEvent | IntegrationEvent | Snapshot | Metadata | Framework;

    public static readonly IReadOnlyDictionary<string, int> ByAttributeName = new Dictionary<string, int>
    {
        ["CommandTypeAttribute"] = Command,
        ["DomainEventTypeAttribute"] = DomainEvent,
        ["IntegrationEventTypeAttribute"] = IntegrationEvent,
        ["SnapshotTypeAttribute"] = Snapshot,
        ["MetadataTypeAttribute"] = Metadata,
        ["FrameworkTypeAttribute"] = Framework,
    };

    /// <summary>Gets the kind declared by <paramref name="attribute"/> when it is a Continuum type map attribute.</summary>
    public static bool TryGetKind(AttributeData attribute, out int kind)
    {
        kind = 0;
        var attributeClass = attribute.AttributeClass;
        return attributeClass is not null
            && ByAttributeName.TryGetValue(attributeClass.Name, out kind)
            && attributeClass.ContainingNamespace?.ToDisplayString() == TypeMappingNamespace;
    }

    /// <summary>Finds the first type map attribute on <paramref name="type"/> whose kind is in <paramref name="kinds"/>.</summary>
    public static AttributeData? Find(INamedTypeSymbol type, int kinds = All)
    {
        foreach (var attribute in type.GetAttributes())
        {
            if (TryGetKind(attribute, out var kind) && (kind & kinds) != 0)
            {
                return attribute;
            }
        }

        return null;
    }
}
