using Continuum.Events;
using Continuum.Orleans.Events;
using Continuum.Orleans.KurrentDB;
using Continuum.Orleans.Streams;
using Continuum.Streaming.Orleans.KurrentDB.StreamProvider;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

using Orleans.Configuration;
using Orleans.Providers.Streams.Common;
using Orleans.Runtime;
using Orleans.Serialization;
using Orleans.Storage;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies how the data adapter handles an Orleans stream id in event metadata that is not well formed, and a commit
///     position that cannot become an Orleans sequence token.
/// </summary>
public class QueueDataAdapterMalformedInputTests
{
    private static readonly Serializer OrleansSerializer = new ServiceCollection().AddSerializer().BuildServiceProvider().GetRequiredService<Serializer>();

    private static readonly StreamName QueueStream = new("queue-0", "queue", null, "0");

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

    private static StreamedEvent<KurrentDBRawEvent> CreateEvent(string? orleansStreamId, ulong commitPosition = 100) =>
        new(Guid.NewGuid(), "test-event", QueueStream, 0, new StreamedEventCursor("test", QueueStream.Category, QueueStream.Value, commitPosition, 0),
            DateTime.UtcNow, new KurrentDBRawEvent([1]),
            orleansStreamId is null ? null : EventMetadata.Create([new KeyValuePair<string, string?>(KurrentDBEventMetadataCodec.OrleansStreamIdName, orleansStreamId)]));

    [Fact]
    public void Falls_Back_To_The_Stream_Name_When_The_Metadata_Stream_Id_Is_Empty()
    {
        var adapter = CreateAdapter();

        Assert.Equal(StreamId.Create("test:queue", "0"), adapter.GetStreamId(CreateEvent(string.Empty)));
    }

    [Fact]
    public void Rejects_A_Metadata_Stream_Id_That_Is_Not_A_Valid_Orleans_Stream_Id()
    {
        var adapter = CreateAdapter();

        Assert.Throws<InvalidOrleansStreamIdException>(() => adapter.GetStreamId(CreateEvent("no-namespace")));
    }

    [Fact]
    public void Rejects_A_Metadata_Stream_Id_Whose_Namespace_Contains_The_Separator()
    {
        var adapter = CreateAdapter();

        Assert.Throws<InvalidStreamNamespaceException>(() => adapter.GetStreamId(CreateEvent("bad:ns/1")));
    }

    [Fact]
    public void Parses_A_Metadata_Stream_Id_Too_Long_For_The_Stack_Buffer()
    {
        var adapter = CreateAdapter();
        var key = new string('k', 1000);

        Assert.Equal(StreamId.Create("test:chat", key), adapter.GetStreamId(CreateEvent(StreamId.Create("chat", key).ToString())));
    }

    [Fact]
    public void Rejects_A_Commit_Position_That_Cannot_Become_An_Orleans_Sequence_Token()
    {
        var adapter = CreateAdapter();

        var exception = Assert.Throws<NotSupportedException>(() => adapter.GetStreamPosition(CreateEvent(null, (ulong)long.MaxValue + 1)));

        Assert.Contains(QueueStream.Value, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Accepts_The_Largest_Commit_Position_That_Fits_An_Orleans_Sequence_Token()
    {
        var adapter = CreateAdapter();

        Assert.Equal(long.MaxValue, adapter.GetStreamPosition(CreateEvent(null, long.MaxValue)).SequenceToken.SequenceNumber);
    }

    [Fact]
    public void The_Cache_Skips_Only_The_Event_With_An_Invalid_Metadata_Stream_Id()
    {
        var adapter = CreateAdapter();
        var cursorTracker = new KurrentDBCursorTracker();
        var evictionStrategy = new KurrentDBEvictionStrategy(NullLogger.Instance, new TimePurgePredicate(TimeSpan.FromHours(1), TimeSpan.FromHours(2)), null, null, adapter, cursorTracker);
        var cache = new KurrentDBQueueCache("queue", 100, new ObjectPool<FixedSizeBuffer>(() => new FixedSizeBuffer(1 << 20)), adapter, evictionStrategy, cursorTracker, NullLogger.Instance, null!, null, null);

        var positions = cache.Add([CreateEvent(null, 100), CreateEvent("no-namespace", 200), CreateEvent(null, 300)], DateTime.UtcNow);

        Assert.Equal([100L, 300L], positions.Select(position => position.SequenceToken.SequenceNumber));
    }
}
