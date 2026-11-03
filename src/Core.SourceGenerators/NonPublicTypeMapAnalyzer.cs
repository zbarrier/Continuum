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
    internal static readonly DiagnosticDescriptor NonPublicTypeMap = new(
        id: "CTM001",
        title: "Type map attribute on a non-public type in a library",
        messageFormat: "'{0}' has a type map attribute but is not publicly accessible; it is only registered once this assembly is loaded. Make it (and any containing types) public so referencing applications register it at startup.",
        category: "Continuum.Core",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Referencing applications generate registrations only for public types in referenced assemblies. Non-public mapped types in a library depend on the library being loaded before a type mapper is created.");

    internal static readonly DiagnosticDescriptor InaccessibleTypeMap = new(
        id: "CTM003",
        title: "Type map attribute on a type the generated registration cannot access",
        messageFormat: "'{0}' has a type map attribute but is private, protected or file-local (or nested in such a type), so it cannot be registered. Make it and any containing types public or internal.",
        category: "Continuum.Core",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The generated module initializer is a separate class in the same assembly and can only reference types visible across the assembly.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(NonPublicTypeMap, InaccessibleTypeMap);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(static start =>
        {
            var isLibrary = start.Compilation.Options.OutputKind == OutputKind.DynamicallyLinkedLibrary;

            start.RegisterSymbolAction(ctx =>
            {
                var type = (INamedTypeSymbol)ctx.Symbol;
                if (!TypeMapRegistrationGenerator.IsRegistrableShape(type) || TypeMapAttributeKinds.Find(type) is null)
                {
                    return;
                }

                DiagnosticDescriptor descriptor;
                if (!TypeMapRegistrationGenerator.IsAccessibleWithinAssembly(type))
                {
                    descriptor = InaccessibleTypeMap;
                }
                else if (isLibrary && !IsPubliclyAccessible(type))
                {
                    descriptor = NonPublicTypeMap;
                }
                else
                {
                    return;
                }

                foreach (var location in type.Locations)
                {
                    if (location.IsInSource)
                    {
                        ctx.ReportDiagnostic(Diagnostic.Create(descriptor, location, type.ToDisplayString()));
                        break;
                    }
                }
            }, SymbolKind.NamedType);
        });
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
