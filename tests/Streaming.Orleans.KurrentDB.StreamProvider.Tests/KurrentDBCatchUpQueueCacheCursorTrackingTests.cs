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
using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.CatchUp;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies that a cursor still reading old events holds back both purging and the checkpoint, and that releasing it
///     lets both advance.
/// </summary>
public class KurrentDBCatchUpQueueCacheCursorTrackingTests
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

    private static KurrentDBCatchUpQueueCache CreateCache(KurrentDBQueueDataAdapter adapter)
    {
        var logger = NullLogger.Instance;
        var bufferPool = new ObjectPool<FixedSizeBuffer>(() => new FixedSizeBuffer(1 << 20));
        var cursorTracker = new KurrentDBCursorTracker();
        // Zero limits purge every event that is older than the newest one, once nothing is still reading it.
        var timePurge = new TimePurgePredicate(TimeSpan.Zero, TimeSpan.Zero);
        var evictionStrategy = new KurrentDBCatchUpEvictionStrategy(logger, timePurge, null, null, adapter, cursorTracker);
        return new KurrentDBCatchUpQueueCache("queue", 100, bufferPool, adapter, evictionStrategy, cursorTracker, logger, null!, null, null);
    }

    private static StreamedEvent<KurrentDBRawEvent> CreateEvent(ulong streamVersion, ulong commitPosition, DateTime timestamp) =>
        new(Guid.NewGuid(), "test-event", Stream, streamVersion, new StreamedEventCursor("test", Stream.Category, Stream.Value, commitPosition, 0),
            timestamp, new KurrentDBRawEvent([1]));

    private static (KurrentDBCatchUpQueueCache Cache, StreamId StreamId, List<ulong> Checkpoints) CreateFilledCache()
    {
        var cache = CreateCache(CreateAdapter());
        var checkpoints = new List<ulong>();
        cache.OnCheckpointablePosition = checkpoints.Add;
        var start = DateTime.UtcNow.AddHours(-1);
        var positions = cache.Add(
        [
            CreateEvent(0, 100, start),
            CreateEvent(1, 200, start.AddMinutes(1)),
            CreateEvent(2, 300, start.AddMinutes(2)),
        ], start.AddMinutes(2));
        return (cache, positions[0].StreamId, checkpoints);
    }

    private static long OldestAvailable(KurrentDBCatchUpQueueCache cache, StreamId streamId)
    {
        var cursor = cache.GetCursorAtPosition(streamId, StreamSubscriptionStartPosition.EarliestAvailable);
        try
        {
            Assert.True(cache.TryGetNextMessage(cursor, out var container));
            return container.SequenceToken.SequenceNumber;
        }
        finally
        {
            cache.ReleaseCursor(cursor);
        }
    }

    [Fact]
    public void A_Cursor_That_Has_Not_Read_Anything_Blocks_Purging_And_Checkpointing()
    {
        var (cache, streamId, checkpoints) = CreateFilledCache();
        cache.GetCursorAtPosition(streamId, StreamSubscriptionStartPosition.EarliestAvailable);

        cache.SignalPurge();

        Assert.Empty(checkpoints);
        Assert.Equal(100, OldestAvailable(cache, streamId));
    }

    [Fact]
    public void A_Cursor_With_Unread_Events_Blocks_Purging_No_Matter_How_Old_They_Are()
    {
        var cache = CreateCache(CreateAdapter());
        var checkpoints = new List<ulong>();
        cache.OnCheckpointablePosition = checkpoints.Add;
        var start = DateTime.UtcNow.AddDays(-30);
        var positions = cache.Add(
        [
            CreateEvent(0, 100, start),
            CreateEvent(1, 200, start.AddDays(1)),
            CreateEvent(2, 300, start.AddDays(2)),
        ], start.AddDays(2));
        var streamId = positions[0].StreamId;
        cache.GetCursorAtPosition(streamId, StreamSubscriptionStartPosition.EarliestAvailable);

        for (var i = 0; i < 5; i++)
        {
            cache.SignalPurge();
        }

        Assert.Empty(checkpoints);
        Assert.Equal(100, OldestAvailable(cache, streamId));
    }

    [Fact]
    public void A_Partly_Read_Cursor_Keeps_Its_Unread_Events_No_Matter_How_Old_They_Are()
    {
        var cache = CreateCache(CreateAdapter());
        var checkpoints = new List<ulong>();
        cache.OnCheckpointablePosition = checkpoints.Add;
        var start = DateTime.UtcNow.AddDays(-30);
        var positions = cache.Add(
        [
            CreateEvent(0, 100, start),
            CreateEvent(1, 200, start.AddDays(1)),
            CreateEvent(2, 300, start.AddDays(2)),
        ], start.AddDays(2));
        var streamId = positions[0].StreamId;
        var cursor = cache.GetCursorAtPosition(streamId, StreamSubscriptionStartPosition.EarliestAvailable);
        cache.RecordDelivered(cursor, 100);

        for (var i = 0; i < 5; i++)
        {
            cache.SignalPurge();
        }

        Assert.Equal([100UL], checkpoints);
        Assert.Equal(200, OldestAvailable(cache, streamId));
    }

    [Fact]
    public void A_Slow_Cursor_Keeps_The_Checkpoint_At_Its_Last_Delivered_Position()
    {
        var (cache, streamId, checkpoints) = CreateFilledCache();
        var cursor = cache.GetCursorAtPosition(streamId, StreamSubscriptionStartPosition.EarliestAvailable);
        cache.RecordDelivered(cursor, 100);

        cache.SignalPurge();

        Assert.Equal([100UL], checkpoints);
        Assert.Equal(200, OldestAvailable(cache, streamId));
    }

    [Fact]
    public void The_Slowest_Of_Several_Cursors_Holds_Back_Purging()
    {
        var (cache, streamId, checkpoints) = CreateFilledCache();
        var fast = cache.GetCursorAtPosition(streamId, StreamSubscriptionStartPosition.EarliestAvailable);
        var slow = cache.GetCursorAtPosition(streamId, StreamSubscriptionStartPosition.EarliestAvailable);
        cache.RecordDelivered(fast, 300);
        cache.RecordDelivered(slow, 100);

        cache.SignalPurge();

        Assert.Equal([100UL], checkpoints);
        Assert.Equal(200, OldestAvailable(cache, streamId));
    }

    [Fact]
    public void Releasing_The_Slow_Cursor_Lets_Purging_And_Checkpointing_Advance()
    {
        var (cache, streamId, checkpoints) = CreateFilledCache();
        var cursor = cache.GetCursorAtPosition(streamId, StreamSubscriptionStartPosition.EarliestAvailable);
        cache.SignalPurge();
        Assert.Empty(checkpoints);

        cache.ReleaseCursor(cursor);
        cache.SignalPurge();

        Assert.Equal([200UL], checkpoints);
        Assert.Equal(300, OldestAvailable(cache, streamId));
    }

    [Fact]
    public void A_Caught_Up_Cursor_Does_Not_Hold_Back_Purging()
    {
        var (cache, streamId, checkpoints) = CreateFilledCache();
        var cursor = cache.GetCursorAtPosition(streamId, StreamSubscriptionStartPosition.EarliestAvailable);

        cache.RecordCaughtUp(cursor);
        cache.SignalPurge();

        Assert.Equal([200UL], checkpoints);
        Assert.Equal(300, OldestAvailable(cache, streamId));
    }

    [Fact]
    public void A_Caught_Up_Cursor_Holds_Back_Events_Added_Later()
    {
        var (cache, streamId, checkpoints) = CreateFilledCache();
        var cursor = cache.GetCursorAtPosition(streamId, StreamSubscriptionStartPosition.EarliestAvailable);
        cache.RecordCaughtUp(cursor);
        var later = DateTime.UtcNow.AddMinutes(-30);
        cache.Add([CreateEvent(3, 400, later), CreateEvent(4, 500, later.AddMinutes(1))], later.AddMinutes(1));

        cache.SignalPurge();

        Assert.Equal([300UL], checkpoints);
        Assert.Equal(400, OldestAvailable(cache, streamId));
    }

    [Fact]
    public void A_Woken_Cursor_Reads_Events_Added_After_Its_Old_Position_Was_Purged()
    {
        var (cache, streamId, _) = CreateFilledCache();
        var cursor = cache.GetCursorAtPosition(streamId, StreamSubscriptionStartPosition.EarliestAvailable);
        while (cache.TryGetNextMessage(cursor, out _))
        {
        }
        cache.RecordCaughtUp(cursor);
        cache.SignalPurge();
        var later = DateTime.UtcNow.AddMinutes(-30);
        cache.Add([CreateEvent(3, 400, later), CreateEvent(4, 500, later.AddMinutes(1))], later.AddMinutes(1));

        cache.SignalPurge();

        Assert.True(cache.TryGetNextMessage(cursor, out var first));
        Assert.Equal(400, first.SequenceToken.SequenceNumber);
        Assert.True(cache.TryGetNextMessage(cursor, out var second));
        Assert.Equal(500, second.SequenceToken.SequenceNumber);
    }

    [Fact]
    public void Waking_A_Cursor_Never_Misses_The_Cache_For_In_Order_Batches()
    {
        var (cache, streamId, _) = CreateFilledCache();
        var cursor = cache.GetCursorAtPosition(streamId, StreamSubscriptionStartPosition.EarliestAvailable);
        var later = DateTime.UtcNow.AddMinutes(-30);
        ulong position = 300;
        for (var batch = 0; batch < 20; batch++)
        {
            while (cache.TryGetNextMessage(cursor, out _))
            {
            }
            cache.RecordCaughtUp(cursor);
            cache.SignalPurge();
            var first = position + 100;
            position += 200;
            var positions = cache.Add([CreateEvent(0, first, later), CreateEvent(0, position, later)], later);

            Assert.Equal(2, positions.Count);
            Assert.True(cache.TryGetNextMessage(cursor, out var next));
            Assert.Equal((long)first, next.SequenceToken.SequenceNumber);
        }
    }

    [Fact]
    public void An_Out_Of_Order_Batch_Older_Than_The_Cache_Still_Returns_Its_Positions()
    {
        var (cache, streamId, _) = CreateFilledCache();
        var cursor = cache.GetCursorAtPosition(streamId, StreamSubscriptionStartPosition.EarliestAvailable);
        while (cache.TryGetNextMessage(cursor, out _))
        {
        }
        cache.RecordCaughtUp(cursor);
        cache.SignalPurge();
        var later = DateTime.UtcNow.AddMinutes(-30);

        // Persistent subscriptions may deliver an event whose commit position is older than everything cached.
        var positions = cache.Add([CreateEvent(5, 50, later)], later);

        Assert.Single(positions);
    }

    [Fact]
    public void A_Woken_Cursor_That_Cannot_Be_Repositioned_Keeps_Reading_From_Its_Old_Cursor()
    {
        var (cache, streamId, _) = CreateFilledCache();
        var cursor = cache.GetCursorAtPosition(streamId, StreamSubscriptionStartPosition.EarliestAvailable);
        while (cache.TryGetNextMessage(cursor, out _))
        {
        }
        cache.RecordCaughtUp(cursor);
        cache.SignalPurge();
        var later = DateTime.UtcNow.AddMinutes(-30);

        // Events at 100 and 200 were purged, so repositioning at 50 misses the cache. The wake must not throw.
        var positions = cache.Add([CreateEvent(5, 50, later)], later);

        Assert.Single(positions);
        Assert.True(cache.TryGetNextMessage(cursor, out var next));
        Assert.Equal(50, next.SequenceToken.SequenceNumber);
    }

    [Fact]
    public void A_Released_Caught_Up_Cursor_Is_Not_Woken()
    {
        var (cache, streamId, checkpoints) = CreateFilledCache();
        var cursor = cache.GetCursorAtPosition(streamId, StreamSubscriptionStartPosition.EarliestAvailable);
        cache.RecordCaughtUp(cursor);
        cache.ReleaseCursor(cursor);
        var later = DateTime.UtcNow.AddMinutes(-30);
        cache.Add([CreateEvent(3, 400, later), CreateEvent(4, 500, later.AddMinutes(1))], later.AddMinutes(1));

        cache.SignalPurge();

        Assert.Equal([400UL], checkpoints);
        Assert.Equal(500, OldestAvailable(cache, streamId));
    }
}
