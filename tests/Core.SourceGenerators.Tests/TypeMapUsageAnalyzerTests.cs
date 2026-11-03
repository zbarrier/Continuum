using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Continuum.Core.SourceGenerators.Tests;

public sealed class TypeMapUsageAnalyzerTests
{
    [Fact]
    public async Task Generic_and_static_classes_report_CTM004()
    {
        var diagnostics = await AnalyzeAsync("""
            [DomainEventType("generic-event")]
            public sealed class GenericEvent<T>;

            [CommandType("static-command")]
            public static class StaticCommand;
            """);

        Assert.Equal(2, diagnostics.Length);
        Assert.All(diagnostics, d => Assert.Equal("CTM004", d.Id));
    }

    [Fact]
    public async Task Duplicate_identifier_reports_CTM005()
    {
        var diagnostics = await AnalyzeAsync("""
            [DomainEventType("order-created")]
            public sealed class OrderCreated;

            [IntegrationEventType("order-created")]
            public sealed class OrderCreatedIntegration;
            """);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CTM005", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("order-created", diagnostic.GetMessage());
    }

    [Fact]
    public async Task Valid_mapped_types_report_nothing()
    {
        var diagnostics = await AnalyzeAsync("""
            [DomainEventType("order-created")]
            public sealed class OrderCreated;

            [CommandType("create-order")]
            public sealed record CreateOrder(int Id);
            """);

        Assert.Empty(diagnostics);
    }

    private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string body)
    {
        var compilation = CompilationFactory.Create(body);
        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new TypeMapUsageAnalyzer());
        return await compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync(TestContext.Current.CancellationToken);
    }
}
