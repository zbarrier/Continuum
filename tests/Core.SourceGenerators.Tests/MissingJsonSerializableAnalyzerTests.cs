using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Continuum.Core.SourceGenerators.Tests;

public sealed class MissingJsonSerializableAnalyzerTests
{
    [Fact]
    public async Task Unlisted_domain_event_reports_CTM002()
    {
        var diagnostics = await AnalyzeAsync("""
            [DomainEventType("order-created")]
            public sealed class OrderCreated;

            [DomainEventType("order-shipped")]
            public sealed class OrderShipped;

            [System.Text.Json.Serialization.JsonSerializable(typeof(OrderCreated))]
            internal sealed partial class AppJsonContext : System.Text.Json.Serialization.JsonSerializerContext;
            """);

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CTM002", diagnostic.Id);
        Assert.Contains("Sample.OrderShipped", diagnostic.GetMessage());
    }

    [Fact]
    public async Task Types_listed_across_multiple_contexts_report_nothing()
    {
        var diagnostics = await AnalyzeAsync("""
            [DomainEventType("order-created")]
            public sealed class OrderCreated;

            [SnapshotType("order-state")]
            public sealed class OrderState;

            [System.Text.Json.Serialization.JsonSerializable(typeof(OrderCreated))]
            internal sealed partial class EventsJsonContext : System.Text.Json.Serialization.JsonSerializerContext;

            [System.Text.Json.Serialization.JsonSerializable(typeof(OrderState))]
            internal sealed partial class SnapshotsJsonContext : System.Text.Json.Serialization.JsonSerializerContext;
            """);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task Project_without_context_reports_nothing()
    {
        var diagnostics = await AnalyzeAsync("""
            [DomainEventType("order-created")]
            public sealed class OrderCreated;
            """);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task Commands_and_metadata_are_not_required()
    {
        var diagnostics = await AnalyzeAsync("""
            [CommandType("create-order")]
            public sealed class CreateOrder;

            [MetadataType("event-metadata")]
            public sealed class EventMetadata;

            [System.Text.Json.Serialization.JsonSerializable(typeof(int))]
            internal sealed partial class AppJsonContext : System.Text.Json.Serialization.JsonSerializerContext;
            """);

        Assert.Empty(diagnostics);
    }

    private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string body)
    {
        // The System.Text.Json generator is not run here, so abstract context members are expected to be missing.
        var compilation = CompilationFactory.Create(body);
        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new MissingJsonSerializableAnalyzer());
        return await compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync(TestContext.Current.CancellationToken);
    }
}
