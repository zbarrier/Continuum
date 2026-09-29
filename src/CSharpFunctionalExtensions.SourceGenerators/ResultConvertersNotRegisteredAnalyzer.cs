using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Continuum.CSharpFunctionalExtensions.SourceGenerators;

/// <summary>
///     Warns when a non-public <c>JsonSerializerContext</c> lists <c>Result</c> types but its generated
///     <c>AddResultConverters</c> method is never called in the compilation, which would fail at runtime under Native AOT.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ResultConvertersNotRegisteredAnalyzer : DiagnosticAnalyzer
{
    private const string JsonSerializableAttribute = "System.Text.Json.Serialization.JsonSerializableAttribute";
    private const string LibraryNamespace = "Continuum.CSharpFunctionalExtensions";
    private const string MethodName = "AddResultConverters";

    internal static readonly DiagnosticDescriptor NotRegistered = new(
        id: "CFE004",
        title: "Result converters are not registered",
        messageFormat: "'{0}' lists Result types but '{0}.AddResultConverters' is never called; Result serialization will fail under Native AOT",
        category: "Continuum.CSharpFunctionalExtensions",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [NotRegistered];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(start =>
        {
            var contexts = new ConcurrentDictionary<INamedTypeSymbol, byte>(SymbolEqualityComparer.Default);
            var called = new ConcurrentDictionary<INamedTypeSymbol, byte>(SymbolEqualityComparer.Default);

            start.RegisterSymbolAction(symbolContext =>
            {
                var type = (INamedTypeSymbol)symbolContext.Symbol;
                if (!IsPubliclyVisible(type) && ListsResultTypes(type))
                {
                    contexts.TryAdd(type, 0);
                }
            }, SymbolKind.NamedType);

            start.RegisterOperationAction(operationContext =>
            {
                var method = ((IInvocationOperation)operationContext.Operation).TargetMethod;
                if (method.Name == MethodName && method.IsStatic)
                {
                    called.TryAdd(method.ContainingType, 0);
                }
            }, OperationKind.Invocation);

            start.RegisterOperationAction(operationContext =>
            {
                var method = ((IMethodReferenceOperation)operationContext.Operation).Method;
                if (method.Name == MethodName && method.IsStatic)
                {
                    called.TryAdd(method.ContainingType, 0);
                }
            }, OperationKind.MethodReference);

            start.RegisterCompilationEndAction(end =>
            {
                foreach (var type in contexts.Keys.Where(t => !called.ContainsKey(t)))
                {
                    end.ReportDiagnostic(Diagnostic.Create(NotRegistered, type.Locations.FirstOrDefault(), type.Name));
                }
            });
        });
    }

    private static bool ListsResultTypes(INamedTypeSymbol type) =>
        type.GetAttributes().Any(a =>
            a.AttributeClass?.ToDisplayString() == JsonSerializableAttribute
            && a.ConstructorArguments.Length > 0
            && a.ConstructorArguments[0].Value is INamedTypeSymbol { Name: "Result", TypeArguments.Length: <= 1 } listed
            && listed.ContainingNamespace.ToDisplayString() == LibraryNamespace);

    private static bool IsPubliclyVisible(INamedTypeSymbol type)
    {
        for (ISymbol? s = type; s is INamedTypeSymbol; s = s.ContainingType)
        {
            if (s.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Protected or Accessibility.ProtectedOrInternal))
            {
                return false;
            }
        }
        return true;
    }
}
