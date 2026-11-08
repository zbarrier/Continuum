using Continuum.Orleans.Streams;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

using Orleans.Configuration;
using Orleans.Providers.Streams.Common;
using Orleans.Serialization;
using Orleans.Storage;
using Orleans.Streams;
using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Catchup;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies that a transaction too large for a cache block halts the subscription, reporting the block size it
///     needs, and nothing past it is checkpointed.
/// </summary>
public class KurrentDBCatchupQueueCacheOversizedEventTests
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

    private static KurrentDBCatchupQueueCache CreateCache()
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
        var evictionStrategy = new KurrentDBCatchupEvictionStrategy(logger, timePurge, null, null, adapter, cursorTracker);
        return new KurrentDBCatchupQueueCache("queue", 100, bufferPool, adapter, evictionStrategy, cursorTracker, logger, null!, null, null);
    }

    private static StreamedEvent<KurrentDBRawEvent> CreateEvent(ulong streamVersion, ulong commitPosition, int dataSize) =>
        new(Guid.NewGuid(), "test-event", Stream, streamVersion, new StreamedEventCursor("test", Stream.Category, Stream.Value, commitPosition, 0),
            DateTime.UtcNow, new KurrentDBRawEvent(new byte[dataSize]));

    [Fact]
    public void An_Oversized_Event_Halts_With_The_Size_Needed()
    {
        var cache = CreateCache();

        var ex = Assert.Throws<KurrentDBTransactionTooLargeForCacheException>(() => cache.Add(
        [
            CreateEvent(0, 100, 10),
            CreateEvent(1, 200, BlockSize * 2),
            CreateEvent(2, 300, 10),
        ], DateTime.UtcNow));

        Assert.Equal(200UL, ex.CommitPosition);
        Assert.Equal(BlockSize, ex.BlockSize);
        Assert.True(ex.Size > BlockSize);
    }

    [Fact]
    public void An_Oversized_Event_Is_Never_Released_For_Checkpointing()
    {
        var cache = CreateCache();
        var checkpoints = new List<ulong>();
        cache.OnCheckpointablePosition = checkpoints.Add;

        Assert.Throws<KurrentDBTransactionTooLargeForCacheException>(() => cache.Add([CreateEvent(0, 100, BlockSize * 2)], DateTime.UtcNow));

        Assert.DoesNotContain(checkpoints, position => position >= 100);
    }
}
