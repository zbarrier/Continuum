using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Continuum.Core.SourceGenerators;

/// <summary>
/// Warns when a type map attribute is applied to a type in a class library that is not publicly accessible.
/// Such types cannot be registered by the generator running in a referencing application, so they are only
/// registered once the library's module initializer runs (when the library is first loaded).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NonPublicTypeMapAnalyzer : DiagnosticAnalyzer
{
    private const string TypeMappingNamespace = "Continuum.TypeMapping";

    private static readonly ImmutableHashSet<string> KnownAttributeNames = ImmutableHashSet.Create(
        "CommandTypeAttribute",
        "DomainEventTypeAttribute",
        "IntegrationEventTypeAttribute",
        "SnapshotTypeAttribute",
        "MetadataTypeAttribute");

    internal static readonly DiagnosticDescriptor NonPublicTypeMap = new(
        id: "CTM001",
        title: "Type map attribute on a non-public type in a library",
        messageFormat: "'{0}' has a type map attribute but is not publicly accessible; it is only registered once this assembly is loaded. Make it (and any containing types) public so referencing applications register it at startup.",
        category: "Continuum.Core",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Referencing applications generate registrations only for public types in referenced assemblies. Non-public mapped types in a library depend on the library being loaded before a type mapper is created.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(NonPublicTypeMap);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(static start =>
        {
            if (start.Compilation.Options.OutputKind != OutputKind.DynamicallyLinkedLibrary)
            {
                return;
            }

            start.RegisterSymbolAction(static ctx =>
            {
                var type = (INamedTypeSymbol)ctx.Symbol;
                if (type.TypeKind != TypeKind.Class || !HasTypeMapAttribute(type) || IsPubliclyAccessible(type))
                {
                    return;
                }

                foreach (var location in type.Locations)
                {
                    if (location.IsInSource)
                    {
                        ctx.ReportDiagnostic(Diagnostic.Create(NonPublicTypeMap, location, type.ToDisplayString()));
                        break;
                    }
                }
            }, SymbolKind.NamedType);
        });
    }

    private static bool HasTypeMapAttribute(INamedTypeSymbol type)
    {
        foreach (var attribute in type.GetAttributes())
        {
            var attributeClass = attribute.AttributeClass;
            if (attributeClass is not null
                && KnownAttributeNames.Contains(attributeClass.Name)
                && attributeClass.ContainingNamespace?.ToDisplayString() == TypeMappingNamespace)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsPubliclyAccessible(INamedTypeSymbol type)
    {
        for (var current = type; current is not null; current = current.ContainingType)
        {
            if (current.DeclaredAccessibility != Accessibility.Public)
            {
                return false;
            }
        }

        return true;
    }
}
