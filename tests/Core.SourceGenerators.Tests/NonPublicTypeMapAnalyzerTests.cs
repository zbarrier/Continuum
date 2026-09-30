using System.Collections.Immutable;

using Continuum.TypeMapping;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Continuum.Core.SourceGenerators.Tests;

public sealed class NonPublicTypeMapAnalyzerTests
{
    [Fact]
    public async Task Internal_mapped_type_in_library_reports_CTM001()
    {
        var diagnostics = await AnalyzeAsync("""
            [DomainEventType("order-created")]
            internal sealed class OrderCreated;
            """);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CTM001", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Contains("Sample.OrderCreated", diagnostic.GetMessage());
    }

    [Fact]
    public async Task Public_type_nested_in_internal_type_reports_CTM001()
    {
        var diagnostics = await AnalyzeAsync("""
            internal static class Outer
            {
                [CommandType("create-order")]
                public sealed class CreateOrder;
            }
            """);

        Assert.Equal("CTM001", Assert.Single(diagnostics).Id);
    }

    [Fact]
    public async Task Public_mapped_type_reports_nothing()
    {
        var diagnostics = await AnalyzeAsync("""
            [SnapshotType("order-state")]
            public sealed class OrderState;

            public static class Outer
            {
                [IntegrationEventType("order-shipped")]
                public sealed class OrderShipped;
            }
            """);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task Internal_unmapped_type_reports_nothing()
    {
        var diagnostics = await AnalyzeAsync("internal sealed class Unmapped;");

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task Internal_mapped_type_in_application_reports_nothing()
    {
        var diagnostics = await AnalyzeAsync("""
            [DomainEventType("order-created")]
            internal sealed class OrderCreated;

            public static class Program
            {
                public static void Main() { }
            }
            """, OutputKind.ConsoleApplication);

        Assert.Empty(diagnostics);
    }

    private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string body, OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary)
    {
        var compilation = CompilationFactory.Create(body, outputKind);

        var compileErrors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(compileErrors);

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new NonPublicTypeMapAnalyzer());
        return await compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync(TestContext.Current.CancellationToken);
    }
}

internal static class CompilationFactory
{
    public static CSharpCompilation Create(string body, OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary)
    {
        var source = $$"""
            using Continuum.TypeMapping;

            namespace Sample;

            {{body}}
            """;

        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(TypeMapAttribute).Assembly.Location));

        return CSharpCompilation.Create(
            "Sample",
            [CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.Current.CancellationToken)],
            references,
            new CSharpCompilationOptions(outputKind));
    }
}
