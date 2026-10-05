using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Analyzers.Tests;

public class GrainTypeCategorySeparatorAnalyzerTests
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

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new GrainTypeCategorySeparatorAnalyzer());
        return await compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Journaled_Grain_With_Separator_Is_Flagged()
    {
        var diagnostics = await AnalyzeAsync("""
            [GrainType("snack-grain")]
            public class SnackGrain : JournaledGrain<State, Event> { }
            """);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CKDB001", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("'snack'", diagnostic.GetMessage());
        Assert.StartsWith("GrainType(", diagnostic.Location.SourceTree!.GetText(TestContext.Current.CancellationToken).ToString(diagnostic.Location.SourceSpan));
    }

    [Fact]
    public async Task Indirectly_Derived_Journaled_Grain_Is_Flagged()
    {
        var diagnostics = await AnalyzeAsync("""
            public abstract class BaseGrain : JournaledGrain<State, Event> { }

            [GrainType("snack-grain")]
            public class SnackGrain : BaseGrain { }
            """);

        Assert.Equal("CKDB001", Assert.Single(diagnostics).Id);
    }

    [Fact]
    public async Task Single_Type_Argument_Journaled_Grain_Is_Flagged()
    {
        var diagnostics = await AnalyzeAsync("""
            [GrainType("snack-grain")]
            public class SnackGrain : JournaledGrain<State> { }
            """);

        Assert.Equal("CKDB001", Assert.Single(diagnostics).Id);
    }

    [Fact]
    public async Task Journaled_Grain_Without_Separator_Is_Not_Flagged()
    {
        var diagnostics = await AnalyzeAsync("""
            [GrainType("snack.grain")]
            public class SnackGrain : JournaledGrain<State, Event> { }

            public class DefaultNamedGrain : JournaledGrain<State, Event> { }
            """);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task Non_Journaled_Grain_With_Separator_Is_Not_Flagged()
    {
        var diagnostics = await AnalyzeAsync("""
            [GrainType("plain-grain")]
            public class PlainGrain : Grain { }
            """);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task Abstract_Journaled_Grain_Is_Not_Flagged()
    {
        var diagnostics = await AnalyzeAsync("""
            [GrainType("base-grain")]
            public abstract class BaseGrain : JournaledGrain<State, Event> { }
            """);

        Assert.Empty(diagnostics);
    }
}
