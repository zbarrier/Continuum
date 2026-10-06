using System.Text;
using System.Text.Json;

using Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests.Grains;

using KurrentDB.Client;

namespace Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests;

/// <summary>
///     Verifies that events written outside the stream provider are delivered to Orleans consumers.
/// </summary>
/// <remarks>
///     The event sourcing storage writes a bare domain event in its own serializer's format and leaves the KurrentDB
///     event metadata empty, whereas the stream provider writes an Orleans serialized envelope with the Orleans stream
///     id in the metadata. Those events are appended here directly, without going through the provider, because that
///     difference in shape is exactly what this test needs to exercise.
/// </remarks>
[Collection(ClusterCollection.Name)]
public class EventSourcedEventDeliveryTests
{
    private const string ConnectionString = "kurrentdb://localhost:2113?tls=false";

    // The name the ChatMessage type is registered under by its DomainEventTypeMap attribute. The provider resolves
    // the CLR type from this rather than from a CLR type name, so events survive class renames.
    private const string EventTypeName = "Tests.ChatMessage";

    private readonly ClusterFixture _fixture;
    private readonly ITestOutputHelper _output;

    public EventSourcedEventDeliveryTests(ClusterFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task Delivers_Events_Written_Outside_The_Stream_Provider()
    {
        // Arrange: the stream name carries the identity, since there is no Orleans metadata to read it from. The
        // default mapper splits on the first '-', so this becomes namespace "eventsourced" and key <guid>.
        var key = Guid.NewGuid();
        var streamName = $"{Constants.EventSourcedStreamPrefix}-{key:N}";
        var streamId = StreamId.Create(Constants.EventSourcedStreamPrefix, key.ToString("N"));

        var collector = _fixture.Cluster.Client.GetGrain<ICollectorGrain>(Guid.NewGuid());
        await collector.Subscribe(streamId);

        var sent = new ChatMessage("EventSourcing", "written without the stream provider", DateTimeOffset.UtcNow);

        // Act
        await using var client = new KurrentDBClient(KurrentDBClientSettings.Create(ConnectionString));
        await AppendAsync(client, streamName, sent);

        var received = await WaitForAsync(collector, expectedCount: 1);

        // Assert: arriving at all proves the event was transcoded into the cache format; the payload proves the
        // conversion used the right type rather than an empty or wrongly shaped instance.
        var message = Assert.Single(received);
        Assert.Equal(sent.Author, message.Author);
        Assert.Equal(sent.Text, message.Text);
    }

    /// <summary>
    ///     Appends a bare JSON domain event with no metadata, the way the event sourcing storage writes.
    /// </summary>
    private static Task AppendAsync(KurrentDBClient client, string streamName, ChatMessage message)
    {
        var data = JsonSerializer.SerializeToUtf8Bytes(message, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var eventData = new EventData(Uuid.NewUuid(), EventTypeName, data);
        return client.AppendToStreamAsync(streamName, StreamState.Any, new[] { eventData });
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
