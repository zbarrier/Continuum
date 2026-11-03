using Continuum.Events;
using Continuum.Orleans.Events;
using Continuum.Orleans.Streams;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;

using Orleans.Configuration;
using Orleans.Serialization;
using Orleans.Storage;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies that every field the data adapter writes into a cache segment is read back unchanged, so the encoder
///     and the KurrentDBMessage cached message constructor cannot drift out of order.
/// </summary>
public class CachedMessageRoundTripTests
{
    private static readonly Serializer OrleansSerializer = new ServiceCollection().AddSerializer().BuildServiceProvider().GetRequiredService<Serializer>();

    private sealed class FakeTypeMapper : TypeMapper;

    private sealed class FakeSerializer : IGrainStorageSerializer
    {
        public BinaryData Serialize<T>(T? input) => throw new NotSupportedException();

        public T Deserialize<T>(BinaryData input) => throw new NotSupportedException();
    }

    private static KurrentDBQueueDataAdapter CreateAdapter() => new(OrleansSerializer, new KurrentDBDataAdapterOptions
    {
        ConnectionName = "test",
        ProviderName = "test",
        TypeMapper = new FakeTypeMapper(),
        GrainStorageSerializer = new FakeSerializer(),
    });

    private static KurrentDBMessage RoundTrip(StreamedEvent<KurrentDBRawEvent> queueMessage)
    {
        var adapter = CreateAdapter();
        var buffer = new byte[4096];
        var cachedMessage = adapter.FromQueueMessage(adapter.GetStreamPosition(queueMessage), queueMessage, DateTime.UtcNow, size => new ArraySegment<byte>(buffer, 0, size));
        return new KurrentDBMessage(cachedMessage, "test");
    }

    private static StreamedEvent<KurrentDBRawEvent> CreateEvent(StreamName streamName, ulong streamVersion, ulong commitPosition, DateTime timestamp, byte[] data, IEventMetadata? metadata = null) =>
        new(Guid.NewGuid(), "snack-added", streamName, streamVersion, new StreamedEventCursor("test", streamName.Category, streamName.Value, commitPosition, 0),
            timestamp, new KurrentDBRawEvent(data), metadata);

    [Fact]
    public void Round_Trips_Every_Field_Of_A_Tenant_Qualified_Event()
    {
        var streamName = new StreamName("snack-acme|123", "snack", "acme", "123");
        var timestamp = new DateTime(2025, 1, 2, 3, 4, 5, DateTimeKind.Utc).AddTicks(6789);
        var metadata = EventMetadata.Create([new KeyValuePair<string, string?>("correlation", "abc")]);
        var queueMessage = CreateEvent(streamName, 42, 9_000_000_000, timestamp, [1, 2, 3], metadata);

        var message = RoundTrip(queueMessage);

        Assert.Equal(9_000_000_000UL, message.CommitPosition);
        Assert.Equal(9_000_000_000L, message.SequenceNumber);
        Assert.Equal(queueMessage.EventId, message.EventId);
        Assert.Equal(StreamId.Create("test:snack", "acme|123"), message.StreamId);
        Assert.Equal(streamName.Value, message.StreamName.Value);
        Assert.Equal(streamName.Category, message.StreamName.Category);
        Assert.Equal(streamName.TenantId, message.StreamName.TenantId);
        Assert.Equal(streamName.Key, message.StreamName.Key);
        Assert.Equal(42UL, message.StreamVersion);
        Assert.Equal("snack-added", message.EventType);
        Assert.Equal(new byte[] { 1, 2, 3 }, message.Data.ToArray());
        Assert.Equal("abc", message.Metadata.GetValueOrDefault("correlation"));
        Assert.Equal(timestamp, message.Timestamp);
        Assert.Equal(DateTimeKind.Utc, message.Timestamp.Kind);
    }

    [Fact]
    public void Round_Trips_An_Event_Without_A_Tenant_Data_Or_Metadata()
    {
        var streamName = new StreamName("snack-123", "snack", null, "123");
        var queueMessage = CreateEvent(streamName, 0, 1, DateTime.UtcNow, []);

        var message = RoundTrip(queueMessage);

        Assert.Equal(1UL, message.CommitPosition);
        Assert.Null(message.StreamName.TenantId);
        Assert.Equal("123", message.StreamName.Key);
        Assert.Equal(0UL, message.StreamVersion);
        Assert.True(message.Data.IsEmpty);
        Assert.Empty(message.Metadata);
    }
}
