using Continuum.Events;
using Continuum.Orleans.Events;
using Continuum.Orleans.Streams;
using Continuum.Streaming.Orleans.KurrentDB.Configuration;
using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Catchup;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

using Orleans.Configuration;
using Orleans.Providers.Streams.Common;
using Orleans.Serialization;
using Orleans.Storage;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies that a transaction only partially received is handled as configured once it is confirmed partial, and
///     is never released while it may still complete.
/// </summary>
public class KurrentDBCatchupQueueCachePartialTransactionTests
{
    private static readonly Serializer OrleansSerializer = new ServiceCollection().AddSerializer().BuildServiceProvider().GetRequiredService<Serializer>();

    private static readonly StreamName Stream = new("test-1", "test", null, "1");

    private sealed class FakeTypeMapper : TypeMapper;

    private sealed class FakeSerializer : IGrainStorageSerializer
    {
        public BinaryData Serialize<T>(T? input) => throw new NotSupportedException();

        public T Deserialize<T>(BinaryData input) => throw new NotSupportedException();
    }

    private static KurrentDBCatchupQueueCache CreateCache(PartialTransactionHandling handling)
    {
        var adapter = new KurrentDBQueueDataAdapter(OrleansSerializer, new KurrentDBDataAdapterOptions
        {
            ConnectionName = "test",
            ProviderName = "test",
            TypeMapper = new FakeTypeMapper(),
            GrainStorageSerializer = new FakeSerializer(),
        });
        var logger = NullLogger.Instance;
        var bufferPool = new ObjectPool<FixedSizeBuffer>(() => new FixedSizeBuffer(1 << 16));
        var cursorTracker = new KurrentDBCursorTracker();
        var timePurge = new TimePurgePredicate(TimeSpan.FromHours(1), TimeSpan.FromHours(2));
        var evictionStrategy = new KurrentDBCatchupEvictionStrategy(logger, timePurge, null, null, adapter, cursorTracker);
        return new KurrentDBCatchupQueueCache("queue", 100, bufferPool, adapter, evictionStrategy, cursorTracker, logger, null!, null, null)
        {
            PartialTransactionHandling = handling,
        };
    }

    private static StreamedEvent<KurrentDBRawEvent> CreateEvent(ulong streamVersion, ulong commitPosition, Guid? transactionId = null, int transactionSize = 3) =>
        new(Guid.NewGuid(), "test-event", Stream, streamVersion, new StreamedEventCursor("test", Stream.Category, Stream.Value, commitPosition, 0),
            DateTime.UtcNow, new KurrentDBRawEvent(new byte[10]),
            transactionId is { } id ? EventMetadata.Create(null, null, null, new TransactionInfo(id, transactionSize, transactionSize, (int)streamVersion)) : null);

    [Fact]
    public void A_Transaction_Still_Being_Read_Is_Not_Cached()
    {
        var cache = CreateCache(PartialTransactionHandling.Deliver);
        var transactionId = Guid.NewGuid();

        var positions = cache.Add([CreateEvent(0, 100, transactionId), CreateEvent(1, 200, transactionId)], DateTime.UtcNow);

        Assert.Empty(positions);
        var pending = Assert.NotNull(cache.PendingTransaction);
        Assert.Equal(transactionId, pending.TransactionId);
        Assert.Equal(200UL, pending.LastPosition);
    }

    [Theory]
    [InlineData(PartialTransactionHandling.Deliver)]
    [InlineData(PartialTransactionHandling.DeliverAndWarn)]
    public void A_Confirmed_Partial_Transaction_Is_Delivered(PartialTransactionHandling handling)
    {
        var cache = CreateCache(handling);
        var transactionId = Guid.NewGuid();
        cache.Add([CreateEvent(0, 100, transactionId), CreateEvent(1, 200, transactionId)], DateTime.UtcNow);

        var positions = cache.ReleasePartialTransaction(DateTime.UtcNow);

        Assert.Single(positions);
        Assert.Null(cache.PendingTransaction);
    }

    [Fact]
    public void A_Confirmed_Partial_Transaction_Halts_By_Default()
    {
        var cache = CreateCache(new KurrentDBAllStreamCatchupSubscriptionReceiverOptions().PartialTransactionHandling);
        var checkpoints = new List<ulong>();
        cache.OnCheckpointablePosition = checkpoints.Add;
        var transactionId = Guid.NewGuid();
        cache.Add([CreateEvent(0, 100, transactionId), CreateEvent(1, 200, transactionId)], DateTime.UtcNow);

        var ex = Assert.Throws<KurrentDBPartialTransactionException>(() => cache.ReleasePartialTransaction(DateTime.UtcNow));

        Assert.Equal(transactionId, ex.TransactionId);
        Assert.Equal(200UL, ex.Position);
        Assert.Equal(2, ex.ReceivedCount);
        Assert.Equal(3, ex.TransactionSize);
        Assert.Empty(checkpoints);
    }

    [Fact]
    public void Another_Event_After_A_Partial_Transaction_Halts_Before_Caching_Anything()
    {
        var cache = CreateCache(PartialTransactionHandling.Halt);
        var checkpoints = new List<ulong>();
        cache.OnCheckpointablePosition = checkpoints.Add;
        var transactionId = Guid.NewGuid();

        Assert.Throws<KurrentDBPartialTransactionException>(() => cache.Add([CreateEvent(0, 100, transactionId), CreateEvent(1, 200)], DateTime.UtcNow));

        Assert.Empty(checkpoints);
        Assert.Equal(transactionId, cache.PendingTransaction?.TransactionId);
    }

    [Fact]
    public void Another_Event_After_A_Partial_Transaction_Delivers_Both_When_Allowed()
    {
        var cache = CreateCache(PartialTransactionHandling.DeliverAndWarn);
        var transactionId = Guid.NewGuid();

        var positions = cache.Add([CreateEvent(0, 100, transactionId), CreateEvent(1, 200)], DateTime.UtcNow);

        Assert.Equal(2, positions.Count);
    }
}
