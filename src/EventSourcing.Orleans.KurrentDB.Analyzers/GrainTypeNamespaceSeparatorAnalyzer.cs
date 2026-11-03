using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Analyzers;

/// <summary>
/// Reports a warning when a grain declares a grain type containing ':'. The KurrentDB stream provider delivers events on
/// the provider-qualified namespace <c>{ProviderName}:{Category}</c>, and the category is normally the grain type, so a
/// grain type containing ':' would be rejected by event-sourcing storage and skipped by the stream provider.
/// </summary>
/// <remarks>
/// A warning rather than an error, because grains that are neither event sourced nor stored in KurrentDB may use ':'.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GrainTypeNamespaceSeparatorAnalyzer : DiagnosticAnalyzer
{
    private const char NamespaceSeparator = ':';

    internal static readonly DiagnosticDescriptor GrainTypeContainsNamespaceSeparator = new(
        id: "CKDB002",
        title: "Grain type contains the reserved stream namespace separator",
        messageFormat: "Grain type '{0}' of grain '{1}' contains ':'; the KurrentDB stream provider reserves ':' to qualify stream namespaces with the provider name. Remove ':' from the grain type. If this grain's type is never used as a KurrentDB stream category, suppress CKDB002 for it.",
        category: "Continuum.EventSourcing.KurrentDB",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The KurrentDB stream provider delivers events on the namespace {ProviderName}:{Category}. A stream category containing ':' is rejected by KurrentDB event-sourcing storage and skipped, with an error log, by the stream provider.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(GrainTypeContainsNamespaceSeparator);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(static start =>
        {
            var grainTypeAttribute = start.Compilation.GetTypeByMetadataName("Orleans.GrainTypeAttribute");
            if (grainTypeAttribute is null)
            {
                return;
            }

            start.RegisterSymbolAction(ctx => AnalyzeType(ctx, grainTypeAttribute), SymbolKind.NamedType);
        });
    }

    private static void AnalyzeType(SymbolAnalysisContext context, INamedTypeSymbol grainTypeAttribute)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (type.TypeKind != TypeKind.Class)
        {
            return;
        }
        foreach (var attribute in type.GetAttributes())
        {
            if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, grainTypeAttribute)
                || attribute.ConstructorArguments.Length != 1
                || attribute.ConstructorArguments[0].Value is not string grainType
                || grainType.IndexOf(NamespaceSeparator) < 0)
            {
                continue;
            }
            var location = attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation() ?? type.Locations[0];
            context.ReportDiagnostic(Diagnostic.Create(GrainTypeContainsNamespaceSeparator, location, grainType, type.ToDisplayString()));
            return;
        }
    }
}
