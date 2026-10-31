using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Continuum.Core.SourceGenerators;

/// <summary>
/// Reports type map attributes the generator cannot register: those on generic or static classes (CTM004), and
/// identifiers shared by more than one type in the same project (CTM005). Duplicates across assemblies can only be
/// detected at runtime, when a type mapper is built.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TypeMapUsageAnalyzer : DiagnosticAnalyzer
{
    internal static readonly DiagnosticDescriptor UnregistrableTypeMap = new(
        id: "CTM004",
        title: "Type map attribute on a generic or static class",
        messageFormat: "'{0}' has a type map attribute but is a generic or static class, so it is not registered",
        category: "Continuum.Core",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Only non-generic, non-static classes and records can be mapped to a stable type identifier.");

    internal static readonly DiagnosticDescriptor DuplicateTypeIdentifier = new(
        id: "CTM005",
        title: "Type map identifier is used by more than one type",
        messageFormat: "Type map identifier '{0}' is used by both '{1}' and '{2}'",
        category: "Continuum.Core",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A type mapper maps each identifier to exactly one type, so a duplicate fails when the mapper is built.",
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(UnregistrableTypeMap, DuplicateTypeIdentifier);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(static start =>
        {
            var identifiers = new ConcurrentDictionary<string, ConcurrentBag<INamedTypeSymbol>>(System.StringComparer.Ordinal);

            start.RegisterSymbolAction(ctx =>
            {
                var type = (INamedTypeSymbol)ctx.Symbol;
                if (TypeMapAttributeKinds.Find(type) is not { } attribute)
                {
                    return;
                }

                if (!TypeMapRegistrationGenerator.IsRegistrableShape(type))
                {
                    Report(ctx.ReportDiagnostic, UnregistrableTypeMap, type, type.ToDisplayString());
                    return;
                }

                if (attribute.ConstructorArguments.Length == 1 && attribute.ConstructorArguments[0].Value is string identifier)
                {
                    identifiers.GetOrAdd(identifier, _ => new ConcurrentBag<INamedTypeSymbol>()).Add(type);
                }
            }, SymbolKind.NamedType);

            start.RegisterCompilationEndAction(ctx =>
            {
                foreach (var entry in identifiers)
                {
                    var types = entry.Value.OrderBy(t => t.ToDisplayString(), System.StringComparer.Ordinal).ToList();
                    for (var i = 1; i < types.Count; i++)
                    {
                        Report(ctx.ReportDiagnostic, DuplicateTypeIdentifier, types[i], entry.Key, types[0].ToDisplayString(), types[i].ToDisplayString());
                    }
                }
            });
        });
    }

    private static void Report(System.Action<Diagnostic> report, DiagnosticDescriptor descriptor, INamedTypeSymbol type, params object[] args)
    {
        foreach (var location in type.Locations)
        {
            if (location.IsInSource)
            {
                report(Diagnostic.Create(descriptor, location, args));
                return;
            }
        }
    }
}
