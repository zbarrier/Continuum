using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Analyzers.Tests;

public class GrainTypeNamespaceSeparatorAnalyzerTests
{
    private const string Preamble = """
        using Orleans;
        using Orleans.EventSourcing;

        namespace Sample;

        public class State { }
        public class Event { }
        """;

    private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string body)
    {
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(global::Orleans.EventSourcing.JournaledGrain<,>).Assembly.Location))
            .Append(MetadataReference.CreateFromFile(typeof(global::Orleans.GrainTypeAttribute).Assembly.Location))
            .Append(MetadataReference.CreateFromFile(typeof(global::Orleans.Grain).Assembly.Location));
        var compilation = CSharpCompilation.Create(
            "Sample",
            [CSharpSyntaxTree.ParseText(Preamble + Environment.NewLine + body, cancellationToken: TestContext.Current.CancellationToken)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var compileErrors = compilation.GetDiagnostics(TestContext.Current.CancellationToken).Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(compileErrors);

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new GrainTypeNamespaceSeparatorAnalyzer());
        return await compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Journaled_Grain_With_Separator_Is_Warned()
    {
        var diagnostics = await AnalyzeAsync("""
            [GrainType("snack:grain")]
            public class SnackGrain : JournaledGrain<State, Event> { }
            """);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CKDB002", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Contains("'snack:grain'", diagnostic.GetMessage());
    }

    [Fact]
    public async Task Plain_Grain_With_Separator_Is_Warned()
    {
        var diagnostics = await AnalyzeAsync("""
            [GrainType("projection:v2")]
            public class ProjectionGrain : Grain { }
            """);

        Assert.Equal("CKDB002", Assert.Single(diagnostics).Id);
    }

    [Fact]
    public async Task Grain_Type_Without_Separator_Is_Not_Flagged()
    {
        var diagnostics = await AnalyzeAsync("""
            [GrainType("snack")]
            public class SnackGrain : JournaledGrain<State, Event> { }
            """);

        Assert.Empty(diagnostics);
    }
}
