using System.Diagnostics;
using System.Text;
using System.Text.Json;

using Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests.Grains;

using KurrentDB.Client;

namespace Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests;

/// <summary>
///     Verifies what the KurrentDB client actually writes into the event metadata slot when an ambient
///     <see cref="Activity" /> is present, and whether the stream provider still delivers such events.
/// </summary>
/// <remarks>
///     The stream provider treats a non-empty metadata slot as proof that it wrote the event itself, and then parses
///     those bytes as an Orleans stream id. The KurrentDB client, however, injects its own tracing context into that
///     same slot as a JSON object containing "$traceId" and "$spanId" whenever an Activity is recording. If that
///     injection happens for event sourcing writes, the discriminator is wrong and those events are misrouted.
///     Existing coverage misses this because it appends with no ambient Activity, so nothing is injected.
/// </remarks>
[Collection(ClusterCollection.Name)]
public class TracingMetadataDiscriminatorTests
{
    private const string ConnectionString = "kurrentdb://localhost:2113?tls=false";

    // The name the ChatMessage type is registered under by its DomainEventTypeMap attribute.
    private const string EventTypeName = "Tests.ChatMessage";

    private readonly ClusterFixture _fixture;
    private readonly ITestOutputHelper _output;

    public TracingMetadataDiscriminatorTests(ClusterFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    /// <summary>
    ///     Establishes the premise: with a recording Activity, the client populates metadata that the caller left null.
    /// </summary>
    [Fact]
    public async Task Client_Injects_Tracing_Context_Into_Metadata_When_An_Activity_Is_Active()
    {
        // Arrange
        var key = Guid.NewGuid();
        var streamName = $"{Constants.EventSourcedStreamPrefix}-{key:N}";
        var message = new ChatMessage("EventSourcing", "written with an ambient activity", DateTimeOffset.UtcNow);

        await using var client = new KurrentDBClient(KurrentDBClientSettings.Create(ConnectionString));

        // Act: append the way the event sourcing storage does, passing null metadata, but inside an Activity.
        using (var listener = StartRecordingListener())
        using (var source = new ActivitySource(nameof(TracingMetadataDiscriminatorTests)))
        using (var activity = source.StartActivity("append", ActivityKind.Producer))
        {
            Assert.NotNull(activity);
            await AppendWithNullMetadataAsync(client, streamName, message);
        }

        // Assert: read the event back and inspect the metadata the server actually stored.
        var stored = await ReadFirstEventAsync(client, streamName);
        var metadata = Encoding.UTF8.GetString(stored.Event.Metadata.ToArray());
        _output.WriteLine($"Stored metadata: '{metadata}'");

        Assert.NotEqual(0, stored.Event.Metadata.Length);
        Assert.Contains("$traceId", metadata, StringComparison.Ordinal);
        Assert.Contains("$spanId", metadata, StringComparison.Ordinal);
    }

    /// <summary>
    ///     Confirms the contrast with the existing test, which appends outside any Activity and sees empty metadata.
    /// </summary>
    [Fact]
    public async Task Client_Leaves_Metadata_Empty_When_No_Activity_Is_Active()
    {
        // Arrange
        var key = Guid.NewGuid();
        var streamName = $"{Constants.EventSourcedStreamPrefix}-{key:N}";
        var message = new ChatMessage("EventSourcing", "written without an ambient activity", DateTimeOffset.UtcNow);

        await using var client = new KurrentDBClient(KurrentDBClientSettings.Create(ConnectionString));

        // Act: no ActivitySource listener, so Activity.Current stays null and there is nothing to inject.
        Assert.Null(Activity.Current);
        await AppendWithNullMetadataAsync(client, streamName, message);

        // Assert
        var stored = await ReadFirstEventAsync(client, streamName);
        _output.WriteLine($"Stored metadata length: {stored.Event.Metadata.Length}");

        Assert.Equal(0, stored.Event.Metadata.Length);
    }

    /// <summary>
    ///     The consequence that matters: an event sourced event written under tracing should still reach consumers.
    /// </summary>
    /// <remarks>
    ///     This failed while the provider used metadata presence as its discriminator, because the injected tracing
    ///     object made the event look provider written and its bytes were then parsed as an Orleans stream id. The
    ///     discriminator is now the presence of the provider's own stream id entry, which tracing cannot forge.
    /// </remarks>
    [Fact]
    public async Task Delivers_Event_Sourced_Event_Written_Under_An_Active_Activity()
    {
        // Arrange
        var key = Guid.NewGuid();
        var streamName = $"{Constants.EventSourcedStreamPrefix}-{key:N}";
        var streamId = StreamId.Create(Constants.EventSourcedStreamPrefix, key.ToString("N"));

        var collector = _fixture.Cluster.Client.GetGrain<ICollectorGrain>(Guid.NewGuid());
        await collector.Subscribe(streamId);

        var sent = new ChatMessage("EventSourcing", "delivered despite tracing metadata", DateTimeOffset.UtcNow);

        // Act
        await using var client = new KurrentDBClient(KurrentDBClientSettings.Create(ConnectionString));
        using (var listener = StartRecordingListener())
        using (var source = new ActivitySource(nameof(TracingMetadataDiscriminatorTests)))
        using (var activity = source.StartActivity("append", ActivityKind.Producer))
        {
            Assert.NotNull(activity);
            await AppendWithNullMetadataAsync(client, streamName, sent);
        }

        var received = await WaitForAsync(collector, expectedCount: 1);

        // Assert
        var message = Assert.Single(received);
        Assert.Equal(sent.Author, message.Author);
        Assert.Equal(sent.Text, message.Text);
    }

    /// <summary>
    ///     Determines whether the client preserves caller supplied metadata or replaces it when injecting tracing.
    /// </summary>
    /// <remarks>
    ///     This is the question that decides whether the metadata slot can be shared. The stream provider writes the
    ///     Orleans stream id there, so if injection replaces rather than merges, the provider is losing its own routing
    ///     data whenever tracing is active, independently of the discriminator problem.
    /// </remarks>
    [Fact]
    public async Task Client_Preserves_Caller_Supplied_Metadata_When_Injecting_Tracing_Context()
    {
        // Arrange: mimic the stream provider, which puts the Orleans stream id in the metadata slot.
        var key = Guid.NewGuid();
        var streamName = $"{Constants.EventSourcedStreamPrefix}-{key:N}";
        var streamId = StreamId.Create(Constants.EventSourcedStreamPrefix, key.ToString("N"));
        var callerMetadata = Encoding.UTF8.GetBytes(streamId.ToString());
        var message = new ChatMessage("StreamProvider", "written with caller metadata", DateTimeOffset.UtcNow);

        await using var client = new KurrentDBClient(KurrentDBClientSettings.Create(ConnectionString));

        // Act
        using (var listener = StartRecordingListener())
        using (var source = new ActivitySource(nameof(TracingMetadataDiscriminatorTests)))
        using (var activity = source.StartActivity("append", ActivityKind.Producer))
        {
            Assert.NotNull(activity);
            var data = JsonSerializer.SerializeToUtf8Bytes(message, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            var eventData = new EventData(Uuid.NewUuid(), EventTypeName, data, callerMetadata, "application/json");
            await client.AppendToStreamAsync(streamName, StreamState.Any, new[] { eventData }, cancellationToken: TestContext.Current.CancellationToken);
        }

        // Assert
        var stored = await ReadFirstEventAsync(client, streamName);
        var metadata = Encoding.UTF8.GetString(stored.Event.Metadata.ToArray());
        _output.WriteLine($"Caller metadata sent: '{streamId}'");
        _output.WriteLine($"Stored metadata: '{metadata}'");

        // If this fails, injection replaced the caller's bytes and the slot cannot be shared.
        Assert.Contains(streamId.ToString(), metadata, StringComparison.Ordinal);
    }

    /// <summary>
    ///     Records the exact shape of the injected tracing metadata so the envelope can read it back reliably.
    /// </summary>
    [Fact]
    public async Task Injected_Tracing_Metadata_Has_A_Documented_Shape()
    {
        // Arrange
        var key = Guid.NewGuid();
        var streamName = $"{Constants.EventSourcedStreamPrefix}-{key:N}";
        var message = new ChatMessage("EventSourcing", "shape probe", DateTimeOffset.UtcNow);

        await using var client = new KurrentDBClient(KurrentDBClientSettings.Create(ConnectionString));

        string expectedTraceId;
        string expectedSpanId;

        // Act
        using (var listener = StartRecordingListener())
        using (var source = new ActivitySource(nameof(TracingMetadataDiscriminatorTests)))
        using (var activity = source.StartActivity("append", ActivityKind.Producer))
        {
            Assert.NotNull(activity);
            expectedTraceId = activity.TraceId.ToHexString();
            expectedSpanId = activity.SpanId.ToHexString();
            await AppendWithNullMetadataAsync(client, streamName, message);
        }

        // Assert: parse as JSON and confirm the keys sit at the root, and that the values match the ambient activity.
        var stored = await ReadFirstEventAsync(client, streamName);
        var metadata = Encoding.UTF8.GetString(stored.Event.Metadata.ToArray());
        _output.WriteLine($"Activity traceId={expectedTraceId} spanId={expectedSpanId}");
        _output.WriteLine($"Stored metadata: '{metadata}'");

        using var document = JsonDocument.Parse(metadata);
        Assert.Equal(JsonValueKind.Object, document.RootElement.ValueKind);

        foreach (var property in document.RootElement.EnumerateObject())
        {
            _output.WriteLine($"  key '{property.Name}' = {property.Value} ({property.Value.ValueKind})");
        }

        Assert.True(document.RootElement.TryGetProperty("$traceId", out var traceId), "$traceId was not a root property.");
        Assert.True(document.RootElement.TryGetProperty("$spanId", out var spanId), "$spanId was not a root property.");

        // The span injected is the one that wraps the append, not necessarily the caller's activity, so only the
        // trace id is required to match. The span id is asserted for shape only.
        Assert.Equal(expectedTraceId, traceId.GetString());
        Assert.False(string.IsNullOrWhiteSpace(spanId.GetString()));
    }

    /// <summary>
    ///     The other half of the shared slot: an event written by the provider must survive tracing injection.
    /// </summary>
    /// <remarks>
    ///     The provider used to write the Orleans stream id as raw bytes in the metadata slot. Because the client
    ///     merges its tracing context into that same slot, those bytes stopped being a parseable stream id as soon as
    ///     an Activity was recording. The id now travels as a named entry inside the JSON document, so injection adds
    ///     to it rather than corrupting it, and routing still works.
    /// </remarks>
    [Fact]
    public async Task Delivers_Provider_Written_Event_Written_Under_An_Active_Activity()
    {
        // Arrange
        var key = Guid.NewGuid();
        var channel = _fixture.Cluster.Client.GetGrain<IChannelGrain>(key);
        var sent = new ChatMessage("Provider", "written through the provider while tracing", DateTimeOffset.UtcNow);

        // The subscription is explicit and starts at the current position, so it has to exist before the publish or
        // the event is gone by the time the collector is listening.
        var streamId = StreamId.Create(Constants.ChatNamespace, key);
        var collector = _fixture.Cluster.Client.GetGrain<ICollectorGrain>(Guid.NewGuid());
        await collector.Subscribe(streamId, Constants.StreamProviderName);

        // Act: publish through the provider itself, with a recording Activity so the client injects tracing.
        using (var listener = StartRecordingListener())
        using (var source = new ActivitySource(nameof(TracingMetadataDiscriminatorTests)))
        using (var activity = source.StartActivity("publish", ActivityKind.Producer))
        {
            Assert.NotNull(activity);
            await channel.SendMessage(sent);
        }

        // Assert: the event still routes to the Orleans stream id the provider recorded.
        var received = await WaitForAsync(collector, expectedCount: 1);
        Assert.Contains(received, message => message.Text == sent.Text);
    }

    /// <summary>
    ///     Appends a bare JSON domain event with null metadata, the way the event sourcing storage writes.
    /// </summary>
    private static Task AppendWithNullMetadataAsync(KurrentDBClient client, string streamName, ChatMessage message)
    {
        var data = JsonSerializer.SerializeToUtf8Bytes(message, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var eventData = new EventData(Uuid.NewUuid(), EventTypeName, data, null, "application/json");
        return client.AppendToStreamAsync(streamName, StreamState.Any, new[] { eventData });
    }

    private static async Task<ResolvedEvent> ReadFirstEventAsync(KurrentDBClient client, string streamName)
    {
        var result = client.ReadStreamAsync(Direction.Forwards, streamName, StreamPosition.Start, maxCount: 1);
        var events = await result.ToListAsync();
        return Assert.Single(events);
    }

    /// <summary>
    ///     An ActivitySource only produces a recording Activity when a listener opts in to sampling it.
    /// </summary>
    private static ActivityListener StartRecordingListener()
    {
        var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == nameof(TracingMetadataDiscriminatorTests),
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
        };
        ActivitySource.AddActivityListener(listener);
        return listener;
    }

    private async Task<ChatMessage[]> WaitForAsync(ICollectorGrain collector, int expectedCount)
    {
        // The $all subscription and the Orleans pulling agent both poll, so delivery is not immediate.
        var deadline = DateTime.UtcNow.AddSeconds(60);
        var received = Array.Empty<ChatMessage>();
        while (DateTime.UtcNow < deadline)
        {
            received = await collector.GetReceived();
            if (received.Length >= expectedCount)
            {
                break;
            }
            await Task.Delay(250);
        }
        _output.WriteLine($"Received {received.Length} event(s).");
        return received;
    }
}
