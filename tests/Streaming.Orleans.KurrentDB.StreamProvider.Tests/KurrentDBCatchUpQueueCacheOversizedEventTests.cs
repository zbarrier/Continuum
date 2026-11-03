using Continuum.Orleans.Streams;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

using Orleans.Configuration;
using Orleans.Providers.Streams.Common;
using Orleans.Serialization;
using Orleans.Storage;
using Orleans.Streams;
using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.CatchUp;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies that an event too large for a cache block is skipped on its own, and the rest of its batch is still
///     cached and delivered.
/// </summary>
public class KurrentDBCatchUpQueueCacheOversizedEventTests
{
    private const int BlockSize = 4096;

    private static readonly Serializer OrleansSerializer = new ServiceCollection().AddSerializer().BuildServiceProvider().GetRequiredService<Serializer>();

    private static readonly StreamName Stream = new("test-1", "test", null, "1");

    private sealed class FakeTypeMapper : TypeMapper;

    private sealed class FakeSerializer : IGrainStorageSerializer
    {
        public BinaryData Serialize<T>(T? input) => throw new NotSupportedException();

        public T Deserialize<T>(BinaryData input) => throw new NotSupportedException();
    }

    private static KurrentDBCatchUpQueueCache CreateCache()
    {
        var adapter = new KurrentDBQueueDataAdapter(OrleansSerializer, new KurrentDBDataAdapterOptions
        {
            ConnectionName = "test",
            ProviderName = "test",
            TypeMapper = new FakeTypeMapper(),
            GrainStorageSerializer = new FakeSerializer(),
        });
        var logger = NullLogger.Instance;
        var bufferPool = new ObjectPool<FixedSizeBuffer>(() => new FixedSizeBuffer(BlockSize));
        var cursorTracker = new KurrentDBCursorTracker();
        var timePurge = new TimePurgePredicate(TimeSpan.FromHours(1), TimeSpan.FromHours(2));
        var evictionStrategy = new KurrentDBCatchUpEvictionStrategy(logger, timePurge, null, null, adapter, cursorTracker);
        return new KurrentDBCatchUpQueueCache("queue", 100, bufferPool, adapter, evictionStrategy, cursorTracker, logger, null!, null, null);
    }

    private static StreamedEvent<KurrentDBRawEvent> CreateEvent(ulong streamVersion, ulong commitPosition, int dataSize) =>
        new(Guid.NewGuid(), "test-event", Stream, streamVersion, new StreamedEventCursor("test", Stream.Category, Stream.Value, commitPosition, 0),
            DateTime.UtcNow, new KurrentDBRawEvent(new byte[dataSize]));

    [Fact]
    public void An_Oversized_Event_Is_Skipped_And_The_Rest_Of_The_Batch_Is_Cached()
    {
        var cache = CreateCache();

        var positions = cache.Add(
        [
            CreateEvent(0, 100, 10),
            CreateEvent(1, 200, BlockSize * 2),
            CreateEvent(2, 300, 10),
        ], DateTime.UtcNow);

        Assert.Equal([100L, 300L], positions.Select(position => position.SequenceToken.SequenceNumber));

        var cursor = cache.GetCursorAtPosition(positions[0].StreamId, StreamSubscriptionStartPosition.EarliestAvailable);
        Assert.True(cache.TryGetNextMessage(cursor, out var first));
        Assert.True(cache.TryGetNextMessage(cursor, out var second));
        Assert.False(cache.TryGetNextMessage(cursor, out _));
        Assert.Equal(100, first.SequenceToken.SequenceNumber);
        Assert.Equal(300, second.SequenceToken.SequenceNumber);
    }

    [Fact]
    public void A_Batch_Of_Only_Oversized_Events_Caches_Nothing()
    {
        var cache = CreateCache();

        var positions = cache.Add([CreateEvent(0, 100, BlockSize * 2)], DateTime.UtcNow);

        Assert.Empty(positions);
    }

    [Fact]
    public void A_Skipped_Batch_On_An_Empty_Cache_Is_Released_For_Checkpointing()
    {
        var cache = CreateCache();
        var checkpoints = new List<ulong>();
        cache.OnCheckpointablePosition = checkpoints.Add;

        cache.Add([CreateEvent(0, 100, BlockSize * 2), CreateEvent(1, 200, BlockSize * 2)], DateTime.UtcNow);

        // Nothing is cached in front of the skipped events, so no purge would ever report them.
        Assert.Equal([200UL], checkpoints);
    }

    [Fact]
    public void A_Skipped_Batch_Behind_Cached_Events_Waits_For_Them_To_Be_Purged()
    {
        var cache = CreateCache();
        var checkpoints = new List<ulong>();
        cache.OnCheckpointablePosition = checkpoints.Add;
        cache.Add([CreateEvent(0, 100, 10)], DateTime.UtcNow);

        cache.Add([CreateEvent(1, 200, BlockSize * 2)], DateTime.UtcNow);

        // The cached event is still in front of the skipped one, so releasing it now would checkpoint past an unread event.
        Assert.Empty(checkpoints);
    }
}
