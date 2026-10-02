using System.Collections.Concurrent;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Continuum.Core.SourceGenerators;

/// <summary>
/// Reports serialized type-mapped classes (domain events, integration events and snapshots) that are not listed through
/// <c>[JsonSerializable]</c> on any <c>JsonSerializerContext</c> in the same project. Only runs when the project declares at least
/// one context, so projects that do not use source-generated JSON are not affected.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MissingJsonSerializableAnalyzer : DiagnosticAnalyzer
{
    private const string TypeMappingNamespace = "Continuum.TypeMapping";
    private const string JsonSerializerContextName = "System.Text.Json.Serialization.JsonSerializerContext";
    private const string JsonSerializableAttributeName = "System.Text.Json.Serialization.JsonSerializableAttribute";

    private static readonly ImmutableHashSet<string> SerializedAttributeNames = ImmutableHashSet.Create(
        "DomainEventTypeAttribute",
        "IntegrationEventTypeAttribute",
        "SnapshotTypeAttribute");

    internal static readonly DiagnosticDescriptor MissingJsonSerializable = new(
        id: "CTM002",
        title: "Type-mapped type is not listed on a JsonSerializerContext",
        messageFormat: "'{0}' has a type map attribute but is not listed with [JsonSerializable] on any JsonSerializerContext in this project; it cannot be serialized under Native AOT",
        category: "Continuum.Core",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Source-generated JSON metadata is only produced for types listed through [JsonSerializable]. Add the type to one of this project's JsonSerializerContext classes; contexts from several projects can be combined with JsonTypeInfoResolver.Combine.",
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(MissingJsonSerializable);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(static start =>
        {
            var contextType = start.Compilation.GetTypeByMetadataName(JsonSerializerContextName);
            var serializableAttribute = start.Compilation.GetTypeByMetadataName(JsonSerializableAttributeName);
            if (contextType is null || serializableAttribute is null)
            {
                return;
            }

            var mappedTypes = new ConcurrentBag<INamedTypeSymbol>();
            var listedTypes = new ConcurrentDictionary<ITypeSymbol, byte>(SymbolEqualityComparer.Default);
            var hasContext = 0;

            start.RegisterSymbolAction(ctx =>
            {
                var type = (INamedTypeSymbol)ctx.Symbol;
                if (type.TypeKind != TypeKind.Class)
                {
                    return;
                }

                if (DerivesFrom(type, contextType))
                {
                    System.Threading.Interlocked.Exchange(ref hasContext, 1);
                    foreach (var attribute in type.GetAttributes())
                    {
                        if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, serializableAttribute)
                            && attribute.ConstructorArguments.Length > 0
                            && attribute.ConstructorArguments[0].Value is ITypeSymbol listed)
                        {
                            listedTypes.TryAdd(listed, 0);
                        }
                    }
                }

                if (HasSerializedTypeMapAttribute(type))
                {
                    mappedTypes.Add(type);
                }
            }, SymbolKind.NamedType);

            start.RegisterCompilationEndAction(ctx =>
            {
                if (hasContext == 0)
                {
                    return;
                }

                foreach (var type in mappedTypes)
                {
                    if (listedTypes.ContainsKey(type))
                    {
                        continue;
                    }

                    foreach (var location in type.Locations)
                    {
                        if (location.IsInSource)
                        {
                            ctx.ReportDiagnostic(Diagnostic.Create(MissingJsonSerializable, location, type.ToDisplayString()));
                            break;
                        }
                    }
                }
            });
        });
    }

    private static bool DerivesFrom(INamedTypeSymbol type, INamedTypeSymbol baseType)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, baseType))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasSerializedTypeMapAttribute(INamedTypeSymbol type)
    {
        foreach (var attribute in type.GetAttributes())
        {
            var attributeClass = attribute.AttributeClass;
            if (attributeClass is not null
                && SerializedAttributeNames.Contains(attributeClass.Name)
                && attributeClass.ContainingNamespace?.ToDisplayString() == TypeMappingNamespace)
            {
                return true;
            }
        }

        return false;
    }
}
