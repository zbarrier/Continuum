using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using Continuum.CSharpFunctionalExtensions.SourceGenerators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Continuum.CSharpFunctionalExtensions.Tests.ResultTests.Json.Serialization;

public class ResultJsonConvertersGeneratorTests
{
    private const string Usings = """
        using System.Text.Json;
        using System.Text.Json.Serialization;
        using Continuum.CSharpFunctionalExtensions;

        """;

    [Fact]
    public void Generates_OneConverterPerListedResultType()
    {
        var (compilation, generated) = Generate(Usings + """
            namespace App;
            public sealed record Order(int Id);
            [JsonSerializable(typeof(Result))]
            [JsonSerializable(typeof(Result<Order>))]
            [JsonSerializable(typeof(Result<int>))]
            [JsonSerializable(typeof(Order))]
            internal partial class AppJsonContext : JsonSerializerContext;
            static class Use { static JsonSerializerOptions O() => AppJsonContext.AddResultConverters(new JsonSerializerOptions()); }
            """);

        var source = Assert.Single(generated);
        Assert.Contains("ResultJsonConverter()", source);
        Assert.Contains("ResultJsonConverter<global::App.Order>()", source);
        Assert.Contains("ResultJsonConverter<int>()", source);
        Assert.Empty(Errors(compilation));
    }

    [Fact]
    public void ContextWithoutResultTypes_GeneratesNothing()
    {
        var (_, generated) = Generate(Usings + """
            [JsonSerializable(typeof(int))]
            internal partial class OtherContext : JsonSerializerContext;
            """);

        Assert.Empty(generated);
    }

    [Fact]
    public void NestedContext_Compiles()
    {
        var (compilation, generated) = Generate(Usings + """
            static partial class Outer
            {
                [JsonSerializable(typeof(Result<string>))]
                internal partial class Inner : JsonSerializerContext;
                static JsonSerializerOptions O() => Inner.AddResultConverters(new JsonSerializerOptions());
            }
            """);

        Assert.Single(generated);
        Assert.Empty(Errors(compilation));
    }

    [Theory]
    [InlineData("internal", false, true)]
    [InlineData("internal", true, false)]
    [InlineData("public", false, false)]
    public async Task CFE004_ReportedOnlyForUncalledNonPublicContexts(string accessibility, bool called, bool expected)
    {
        var call = called ? "static class Use { static JsonSerializerOptions O() => Ctx.AddResultConverters(new JsonSerializerOptions()); }" : "";
        var (compilation, _) = Generate(Usings + $$"""
            [JsonSerializable(typeof(Result<int>))]
            {{accessibility}} partial class Ctx : JsonSerializerContext;
            {{call}}
            """);

        var diagnostics = await compilation
            .WithAnalyzers([new ResultConvertersNotRegisteredAnalyzer()])
            .GetAnalyzerDiagnosticsAsync(TestContext.Current.CancellationToken);

        Assert.Equal(expected, diagnostics.Any(d => d.Id == "CFE004"));
    }

    [Fact]
    public void GeneratedMethod_RegistersWorkingConverters()
    {
        var options = TestJsonContext.AddResultConverters(new JsonSerializerOptions { TypeInfoResolver = TestJsonContext.Default });

        var json = JsonSerializer.Serialize(Result.Success(7), options);
        var back = JsonSerializer.Deserialize<Result<int>>(json, options);

        Assert.True(back.IsSuccess);
        Assert.Equal(7, back.Value);
    }

    // The System.Text.Json generator does not run in these compilations, so the context's abstract members
    // (CS0534) and base constructor (CS7036) are missing; any other error is a real problem.
    private static IEnumerable<Diagnostic> Errors(Compilation compilation) =>
        compilation.GetDiagnostics(TestContext.Current.CancellationToken)
            .Where(d => d.Severity == DiagnosticSeverity.Error && d.Id is not ("CS0534" or "CS7036"));

    private static (Compilation Compilation, ImmutableArray<string> Generated) Generate(string source)
    {
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(Result).Assembly.Location));

        var compilation = CSharpCompilation.Create("GeneratorTest", [CSharpSyntaxTree.ParseText(source)], references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var driver = CSharpGeneratorDriver.Create(new ResultJsonConvertersGenerator())
            .RunGeneratorsAndUpdateCompilation(compilation, out var output, out _);

        var generated = driver.GetRunResult().GeneratedTrees.Select(t => t.ToString()).ToImmutableArray();
        return (output, generated);
    }
}

[System.Text.Json.Serialization.JsonSerializable(typeof(Result<int>))]
internal partial class TestJsonContext : System.Text.Json.Serialization.JsonSerializerContext;
