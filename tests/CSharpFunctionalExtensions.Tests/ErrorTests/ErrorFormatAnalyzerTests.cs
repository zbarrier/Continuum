using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Continuum.CSharpFunctionalExtensions.SourceGenerators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Continuum.CSharpFunctionalExtensions.Tests.ErrorTests;

public class ErrorFormatAnalyzerTests
{
    [Theory]
    [InlineData("Order {0} was not found.", 1)]
    [InlineData("Order {0} line {1} has quantity {2:F2}.", 3)]
    [InlineData("No placeholders.", 0)]
    [InlineData("Escaped {{0}} braces.", 0)]
    [InlineData("Aligned {0,-10} and {1, 5:N0}.", 2)]
    [InlineData("Index with trailing space {0 }.", 1)]
    [InlineData("Repeated {0} {0}.", 1)]
    [InlineData("Order {1} was not found.", 1)]
    [InlineData("Order {0} line {1}.", 1)]
    [InlineData("Order {0} was not found.", 0)]
    [InlineData("Unclosed {0", 1)]
    [InlineData("Stray } brace", 0)]
    [InlineData("Empty {} placeholder", 0)]
    [InlineData("Named {orderId} placeholder", 1)]
    [InlineData("Leading space { 0}", 1)]
    [InlineData("Bad alignment {0,x}", 1)]
    [InlineData("Brace in specifier {0:{}", 1)]
    [InlineData("Trailing {", 0)]
    public async Task ConstantFormat_MatchesStringFormat(string format, int argumentCount)
    {
        var arguments = string.Concat(Enumerable.Range(0, argumentCount).Select(i => $", \"a{i}\""));
        var throws = Throws(format, argumentCount);

        var diagnostics = await Analyze($"RequestErrors.NewNotFound({Literal(format)}{arguments})");

        Assert.Equal(throws ? ["CFE005"] : [], diagnostics.Select(d => d.Id));
    }

    [Theory]
    [InlineData("new RequestError(HttpStatusCode.NotFound, StatusCode.NotFound, \"c\", \"Order {1}.\", new ErrorArgument[] { \"a\" })")]
    [InlineData("new RequestError(HttpStatusCode.NotFound, StatusCode.NotFound, \"c\", \"Order {0}.\", null, null, null)")]
    [InlineData("new RequestError(HttpStatusCode.NotFound, StatusCode.NotFound, \"c\", \"Order {0}.\", [], null, null)")]
    [InlineData("RequestErrors.New(HttpStatusCode.NotFound, StatusCode.NotFound, Template, \"a\")")]
    [InlineData("new ValidationErrorEntry(ValidationSeverity.Error, \"Name\", \"c\", \"{0} must be at most {1} characters.\", new ErrorArgument[] { \"Name\" })")]
    [InlineData("new ValidationErrorEntry(ValidationSeverity.Error, \"Name\", \"c\", \"{0} is {invalid}.\", null)")]
    public async Task InvalidConstantFormat_OtherShapes_ReportsCFE005(string expression)
    {
        var diagnostics = await Analyze(expression);

        Assert.Equal(["CFE005"], diagnostics.Select(d => d.Id));
    }

    [Theory]
    [InlineData("RequestErrors.NewNotFound(format, \"a\")")]
    [InlineData("RequestErrors.NewNotFound(\"Order {5}.\", args)")]
    [InlineData("RequestErrors.NewNotFound()")]
    [InlineData("RequestErrors.NewNotFound(null)")]
    [InlineData("new RequestError(HttpStatusCode.NotFound, StatusCode.NotFound, \"c\")")]
    [InlineData("new ValidationErrorEntry(ValidationSeverity.Error, \"Name\", \"c\", \"{0} is required.\", new ErrorArgument[] { \"Name\" })")]
    [InlineData("new ValidationErrorEntry(ValidationSeverity.Error, \"Name\", \"c\", \"{0} is required.\", args)")]
    public async Task UnknownOrMissingFormat_NoDiagnostics(string expression)
    {
        var diagnostics = await Analyze(expression);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("RequestErrors.NewNotFound($\"Order {id} was not found.\")")]
    [InlineData("RequestErrors.NewNotFound(\"Order \" + id + \" was not found.\")")]
    [InlineData("new RequestError(HttpStatusCode.NotFound, StatusCode.NotFound, \"c\", $\"Order {id}.\")")]
    [InlineData("new ValidationErrorEntry(ValidationSeverity.Error, \"Name\", \"c\", $\"Order {id} is invalid.\", null)")]
    public async Task FormatBuiltPerCall_ReportsCFE006(string expression)
    {
        var diagnostics = await Analyze(expression);

        Assert.Equal(["CFE006"], diagnostics.Select(d => d.Id));
    }

    private static bool Throws(string format, int argumentCount)
    {
        try
        {
            _ = string.Format(CultureInfo.InvariantCulture, format, Enumerable.Range(0, argumentCount).Select(i => (object)$"a{i}").ToArray());
            return false;
        }
        catch (FormatException)
        {
            return true;
        }
    }

    private static string Literal(string value) => SymbolDisplay.FormatLiteral(value, quote: true);

    private static async Task<ImmutableArray<Diagnostic>> Analyze(string expression)
    {
        var source = $$"""
            using System;
            using System.Net;
            using Continuum.CSharpFunctionalExtensions;
            using Grpc.Core;

            static class Sample
            {
                const string Template = "Order {1}.";

                static object Create(string format, ErrorArgument[] args, Guid id) => {{expression}};
            }
            """;

        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(RequestError).Assembly.Location))
            .Append(MetadataReference.CreateFromFile(typeof(Grpc.Core.StatusCode).Assembly.Location));

        var compilation = CSharpCompilation.Create("AnalyzerTest", [CSharpSyntaxTree.ParseText(source)], references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var compileErrors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        Assert.Empty(compileErrors);

        return await compilation
            .WithAnalyzers([new ErrorFormatAnalyzer()])
            .GetAnalyzerDiagnosticsAsync();
    }
}
