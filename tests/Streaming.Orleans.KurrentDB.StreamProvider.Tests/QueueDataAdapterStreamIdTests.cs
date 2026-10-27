using Continuum.Events;
using Continuum.Orleans.Events;
using Continuum.Orleans.KurrentDB;
using Continuum.Orleans.Streams;
using Continuum.Streaming.Orleans.KurrentDB.StreamProvider;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;

using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Serialization;
using Orleans.Storage;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies where the data adapter takes an event's Orleans stream id from: the metadata entry this provider's
///     producer writes, or the KurrentDB stream name for events written by anyone else.
/// </summary>
public class QueueDataAdapterStreamIdTests
{
    private static readonly Serializer OrleansSerializer = new ServiceCollection().AddSerializer().BuildServiceProvider().GetRequiredService<Serializer>();

    private sealed class FakeTypeMapper : TypeMapper;

    private sealed class FakeSerializer : IGrainStorageSerializer
    {
        public BinaryData Serialize<T>(T? input) => throw new NotSupportedException();

        public T Deserialize<T>(BinaryData input) => throw new NotSupportedException();
    }

    private static KurrentDBQueueDataAdapter CreateAdapter(Func<string, StreamId>? streamIdMapper = null)
    {
        var options = new KurrentDBDataAdapterOptions
        {
            ConnectionName = "test",
            TypeMapper = new FakeTypeMapper(),
            GrainStorageSerializer = new FakeSerializer(),
        };
        if (streamIdMapper is not null)
        {
            options.StreamIdMapper = streamIdMapper;
        }
        return new KurrentDBQueueDataAdapter(OrleansSerializer, options);
    }

    private static StreamedEvent<KurrentDBRawEvent> CreateEvent(StreamName streamName, IEventMetadata? metadata = null) =>
        new(Guid.NewGuid(), "test-event", streamName, 0, new StreamedEventCursor("test", streamName.Category, streamName.Value, 100, 0),
            DateTime.UtcNow, new KurrentDBRawEvent([1]), metadata);

    private static IEventMetadata WithOrleansStreamId(StreamId streamId) =>
        EventMetadata.Create([new KeyValuePair<string, string?>(KurrentDBEventMetadataCodec.OrleansStreamIdName, streamId.ToString())]);

    [Fact]
    public void Uses_The_Stream_Name_When_The_Event_Has_No_Orleans_Stream_Id()
    {
        var adapter = CreateAdapter();
        var queueMessage = CreateEvent(new StreamName("snack-acme|123", "snack", "acme", "123"));

        Assert.Equal(StreamId.Create("snack", "acme|123"), adapter.GetStreamId(queueMessage));
    }

    [Fact]
    public void Uses_The_Orleans_Stream_Id_From_Metadata_When_Present()
    {
        var adapter = CreateAdapter();
        var producerStreamId = StreamId.Create("chat", "acme|room-7");
        // The producer appends every stream to one shared queue stream, so its name says nothing about the Orleans stream.
        var queueMessage = CreateEvent(new StreamName("queue-0", "queue", null, "0"), WithOrleansStreamId(producerStreamId));

        Assert.Equal(producerStreamId, adapter.GetStreamId(queueMessage));
    }

    [Fact]
    public void Metadata_Takes_Precedence_Over_A_Custom_Stream_Id_Mapper()
    {
        var adapter = CreateAdapter(_ => throw new InvalidOperationException("The mapper must not be used when metadata carries the stream id."));
        var producerStreamId = StreamId.Create("chat", "room-7");
        var queueMessage = CreateEvent(new StreamName("queue-0", "queue", null, "0"), WithOrleansStreamId(producerStreamId));

        Assert.Equal(producerStreamId, adapter.GetStreamId(queueMessage));
    }

    [Fact]
    public void Uses_A_Custom_Stream_Id_Mapper_For_Events_Without_Metadata()
    {
        var mapped = StreamId.Create("custom", "mapped");
        var adapter = CreateAdapter(_ => mapped);
        var queueMessage = CreateEvent(new StreamName("snack-123", "snack", null, "123"));

        Assert.Equal(mapped, adapter.GetStreamId(queueMessage));
    }

    [Fact]
    public void Stream_Position_Uses_The_Resolved_Stream_Id()
    {
        var adapter = CreateAdapter();
        var queueMessage = CreateEvent(new StreamName("snack-acme|123", "snack", "acme", "123"));

        Assert.Equal(StreamId.Create("snack", "acme|123"), adapter.GetStreamPosition(queueMessage).StreamId);
    }
}
