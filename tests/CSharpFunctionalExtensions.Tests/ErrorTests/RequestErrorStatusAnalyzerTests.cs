using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Continuum.CSharpFunctionalExtensions.SourceGenerators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Continuum.CSharpFunctionalExtensions.Tests.ErrorTests;

public class RequestErrorStatusAnalyzerTests
{
    [Theory]
    [InlineData("new RequestError(HttpStatusCode.NotFound, StatusCode.NotFound, \"c\")")]
    [InlineData("new RequestError(HttpStatusCode.Gone, StatusCode.NotFound, \"c\")")]
    [InlineData("RequestErrors.New(HttpStatusCode.Conflict, StatusCode.Aborted)")]
    [InlineData("new RequestError(HttpStatusCode.ServiceUnavailable, StatusCode.Unavailable, \"c\", null, null, null, TimeSpan.FromSeconds(5))")]
    [InlineData("new RequestError(HttpStatusCode.BadRequest, StatusCode.InvalidArgument, \"c\", null, null, null, null)")]
    [InlineData("new RequestError(http, StatusCode.NotFound, \"c\")")]
    public async Task RecommendedOrNonConstant_NoDiagnostics(string expression)
    {
        var diagnostics = await Analyze(expression);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("new RequestError(HttpStatusCode.OK, StatusCode.NotFound, \"c\")")]
    [InlineData("new RequestError(HttpStatusCode.NotFound, StatusCode.Internal, \"c\")")]
    [InlineData("RequestErrors.New(HttpStatusCode.BadRequest, StatusCode.Unavailable)")]
    [InlineData("new RequestError((HttpStatusCode)418, StatusCode.InvalidArgument, \"c\")")]
    public async Task UnusualPair_ReportsCFE002(string expression)
    {
        var diagnostics = await Analyze(expression);

        Assert.Equal(["CFE002"], diagnostics.Select(d => d.Id));
    }

    [Theory]
    [InlineData("new RequestError(HttpStatusCode.BadRequest, StatusCode.InvalidArgument, \"c\", null, null, null, TimeSpan.FromSeconds(5))")]
    [InlineData("new RequestError(HttpStatusCode.NotFound, StatusCode.NotFound, \"c\", null, null, null, retryAfter: TimeSpan.Zero)")]
    public async Task RetryAfterOnNonRetryable_ReportsCFE003(string expression)
    {
        var diagnostics = await Analyze(expression);

        Assert.Equal(["CFE003"], diagnostics.Select(d => d.Id));
    }

    private static async Task<ImmutableArray<Diagnostic>> Analyze(string expression)
    {
        var source = $$"""
            using System;
            using System.Net;
            using Continuum.CSharpFunctionalExtensions;
            using Grpc.Core;

            static class Sample
            {
                static object Create(HttpStatusCode http) => {{expression}};
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
            .WithAnalyzers([new RequestErrorStatusAnalyzer()])
            .GetAnalyzerDiagnosticsAsync();
    }
}
