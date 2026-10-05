using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Analyzers;

/// <summary>
/// Reports an error when a journaled grain declares a grain type containing '-'. KurrentDB derives a stream's category from the
/// text before the first '-', and the default stream name is <c>{GrainType}-{GrainKey}</c>, so such a grain's events
/// would be filed under a truncated category.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GrainTypeCategorySeparatorAnalyzer : DiagnosticAnalyzer
{
    private const char CategorySeparator = '-';

    internal static readonly DiagnosticDescriptor GrainTypeContainsCategorySeparator = new(
        id: "CKDB001",
        title: "Journaled grain type contains the KurrentDB category separator",
        messageFormat: "Grain type '{0}' of journaled grain '{1}' contains '-'; KurrentDB would use '{2}' as the stream category. Remove '-' from the grain type. If this grain is not stored by a KurrentDB provider, suppress CKDB001 for it.",
        category: "Continuum.EventSourcing.KurrentDB",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "KurrentDB derives a stream's category from the text before the first '-'. The default stream name is {GrainType}-{GrainKey}, so a grain type containing '-' places its events in the wrong category and breaks $ce- category subscriptions.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(GrainTypeContainsCategorySeparator);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(static start =>
        {
            var journaledGrain = start.Compilation.GetTypeByMetadataName("Orleans.EventSourcing.JournaledGrain`2");
            var grainTypeAttribute = start.Compilation.GetTypeByMetadataName("Orleans.GrainTypeAttribute");
            if (journaledGrain is null || grainTypeAttribute is null)
            {
                return;
            }

            start.RegisterSymbolAction(ctx => AnalyzeType(ctx, journaledGrain, grainTypeAttribute), SymbolKind.NamedType);
        });
    }

    private static void AnalyzeType(SymbolAnalysisContext context, INamedTypeSymbol journaledGrain, INamedTypeSymbol grainTypeAttribute)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (type.TypeKind != TypeKind.Class || type.IsAbstract || !DerivesFrom(type, journaledGrain))
        {
            return;
        }
        foreach (var attribute in type.GetAttributes())
        {
            if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, grainTypeAttribute)
                || attribute.ConstructorArguments.Length != 1
                || attribute.ConstructorArguments[0].Value is not string grainType)
            {
                continue;
            }
            var index = grainType.IndexOf(CategorySeparator);
            if (index < 0)
            {
                return;
            }
            var location = attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation() ?? type.Locations[0];
            context.ReportDiagnostic(Diagnostic.Create(GrainTypeContainsCategorySeparator, location, grainType, type.ToDisplayString(), grainType.Substring(0, index)));
            return;
        }
    }

    private static bool DerivesFrom(INamedTypeSymbol type, INamedTypeSymbol baseDefinition)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, baseDefinition))
            {
                return true;
            }
        }
        return false;
    }
}
