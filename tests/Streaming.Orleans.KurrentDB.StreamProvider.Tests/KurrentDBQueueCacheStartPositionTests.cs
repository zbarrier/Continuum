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
using Orleans.Streams;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies how a cursor is positioned once older events have been purged: an earliest available subscription
///     starts at the oldest cached event, while an exact position that was purged reports a cache miss.
/// </summary>
public class KurrentDBQueueCacheStartPositionTests
{
    private static readonly Serializer OrleansSerializer = new ServiceCollection().AddSerializer().BuildServiceProvider().GetRequiredService<Serializer>();

    private static readonly StreamName Stream = new("test-1", "test", null, "1");

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

    private static KurrentDBQueueCache CreateCache(KurrentDBQueueDataAdapter adapter)
    {
        var logger = NullLogger.Instance;
        var bufferPool = new ObjectPool<FixedSizeBuffer>(() => new FixedSizeBuffer(1 << 20));
        var cursorTracker = new KurrentDBCursorTracker();
        // Zero limits purge every event that is older than the newest one, once nothing is still reading it.
        var timePurge = new TimePurgePredicate(TimeSpan.Zero, TimeSpan.Zero);
        var evictionStrategy = new KurrentDBEvictionStrategy(logger, timePurge, null, null, adapter, cursorTracker);
        return new KurrentDBQueueCache("queue", 100, bufferPool, adapter, evictionStrategy, cursorTracker, logger, null!, null, null);
    }

    private static StreamedEvent<KurrentDBRawEvent> CreateEvent(ulong streamVersion, ulong commitPosition, DateTime timestamp) =>
        new(Guid.NewGuid(), "test-event", Stream, streamVersion, new StreamedEventCursor("test", Stream.Category, Stream.Value, commitPosition, 0),
            timestamp, new KurrentDBRawEvent([1]));

    private static (KurrentDBQueueCache Cache, StreamId StreamId) CreateCacheWithOldestEventsPurged()
    {
        var adapter = CreateAdapter();
        var cache = CreateCache(adapter);
        var start = DateTime.UtcNow.AddHours(-1);
        var positions = cache.Add(
        [
            CreateEvent(0, 100, start),
            CreateEvent(1, 200, start.AddMinutes(1)),
            CreateEvent(2, 300, start.AddMinutes(2)),
        ], start.AddMinutes(2));
        cache.SignalPurge();
        return (cache, positions[0].StreamId);
    }

    [Fact]
    public void Earliest_Available_Starts_At_The_Oldest_Cached_Event_After_A_Purge()
    {
        var (cache, streamId) = CreateCacheWithOldestEventsPurged();

        var cursor = cache.GetCursorAtPosition(streamId, StreamSubscriptionStartPosition.EarliestAvailable);

        Assert.True(cache.TryGetNextMessage(cursor, out var container));
        Assert.Equal(300, container.SequenceToken.SequenceNumber);
    }

    [Fact]
    public void An_Exact_Position_That_Was_Purged_Reports_A_Cache_Miss()
    {
        var (cache, streamId) = CreateCacheWithOldestEventsPurged();

        Assert.Throws<QueueCacheMissException>(() => cache.GetCursor(streamId, new EventSequenceToken(100, 0)));
    }

    [Fact]
    public void Reads_The_Commit_Position_From_A_Cached_Message()
    {
        var adapter = CreateAdapter();
        var queueMessage = CreateEvent(7, 300, DateTime.UtcNow);
        var buffer = new byte[4096];

        var cachedMessage = adapter.FromQueueMessage(adapter.GetStreamPosition(queueMessage), queueMessage, DateTime.UtcNow, size => new ArraySegment<byte>(buffer, 0, size));

        Assert.Equal(300UL, adapter.GetCommitPosition(cachedMessage));
    }
}
