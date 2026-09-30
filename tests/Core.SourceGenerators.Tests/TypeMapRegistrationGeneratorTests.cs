using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Continuum.Core.SourceGenerators.Tests;

public sealed class TypeMapRegistrationGeneratorTests
{
    [Fact]
    public void Generates_registrations_for_mapped_types_only()
    {
        var compilation = CompilationFactory.Create("""
            [DomainEventType("order-created")]
            public sealed class OrderCreated;

            [CommandType("create-order")]
            internal sealed class CreateOrder;

            public sealed class Unmapped;

            [DomainEventType("order-shipped")]
            public sealed record OrderShipped(int Id);

            [MetadataType("event-metadata")]
            public sealed class EventMetadata;
            """);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new TypeMapRegistrationGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics, TestContext.Current.CancellationToken);

        Assert.Empty(diagnostics);
        Assert.Empty(output.GetDiagnostics(TestContext.Current.CancellationToken).Where(d => d.Severity == DiagnosticSeverity.Error));

        var generated = string.Concat(driver.GetRunResult().GeneratedTrees.Select(t => t.ToString()));
        Assert.Contains("\"order-created\"", generated);
        Assert.Contains("\"create-order\"", generated);
        Assert.Contains("Sample.OrderCreated", generated);
        Assert.Contains("Sample.CreateOrder", generated);
        Assert.Contains("Sample.OrderShipped", generated);
        Assert.Contains("\"event-metadata\", (global::Continuum.TypeMapping.TypeMapKinds)16", generated);
        Assert.DoesNotContain("Unmapped", generated);
    }

    [Fact]
    public void Generates_registrations_for_public_types_in_referenced_libraries()
    {
        var library = CompilationFactory.Create("""
            [SnapshotType("lib.public")]
            public sealed class PublicSnapshot;

            [DomainEventType("lib.internal")]
            internal sealed class InternalEvent;
            """);
        using var stream = new MemoryStream();
        Assert.True(library.Emit(stream, cancellationToken: TestContext.Current.CancellationToken).Success);
        var libraryReference = MetadataReference.CreateFromImage(stream.ToArray());

        var app = CompilationFactory.Create("public sealed class Local;").AddReferences(libraryReference);

        var generated = Run(CSharpGeneratorDriver.Create(new TypeMapRegistrationGenerator()), app, out _);

        Assert.Contains("\"lib.public\"", generated);
        Assert.DoesNotContain("lib.internal", generated);
    }

    [Fact]
    public void Unrelated_edit_does_not_regenerate_output()
    {
        var compilation = CompilationFactory.Create("""
            [DomainEventType("order-created")]
            public sealed class OrderCreated;
            """);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new TypeMapRegistrationGenerator().AsSourceGenerator()],
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));

        Run(driver, compilation, out driver);

        var edited = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(
            "namespace Sample; public sealed class Unrelated { }", cancellationToken: TestContext.Current.CancellationToken));
        driver = driver.RunGenerators(edited, TestContext.Current.CancellationToken);

        var outputs = driver.GetRunResult().Results.Single().TrackedOutputSteps.SelectMany(s => s.Value).SelectMany(s => s.Outputs);
        Assert.All(outputs, o => Assert.True(o.Reason is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged, o.Reason.ToString()));
    }

    private static string Run(GeneratorDriver driver, Compilation compilation, out GeneratorDriver updated)
    {
        updated = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics, TestContext.Current.CancellationToken);

        Assert.Empty(diagnostics);
        Assert.Empty(output.GetDiagnostics(TestContext.Current.CancellationToken).Where(d => d.Severity == DiagnosticSeverity.Error));

        return string.Concat(updated.GetRunResult().GeneratedTrees.Select(t => t.ToString()));
    }
}
