using System.Text;

using Continuum.Events;
using Continuum.Orleans.Events;
using Continuum.Orleans.Streams;
using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Catchup;
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
///     Verifies that a transaction is fanned out to its stream targets by scope: a transaction-scoped target receives
///     every event, and an event-scoped target only the events that name it.
/// </summary>
public class KurrentDBCatchupQueueCacheStreamTargetTests
{
    private const string EventType = "test-event";

    private static readonly Serializer OrleansSerializer = new ServiceCollection().AddSerializer().BuildServiceProvider().GetRequiredService<Serializer>();

    private sealed record NamedEvent(string Name);

    private sealed class FakeTypeMapper : TypeMapper;

    // Each event's payload is its name, so a delivered batch shows which events reached a stream.
    private sealed class NameSerializer : IGrainStorageSerializer
    {
        public BinaryData Serialize<T>(T? input) => throw new NotSupportedException();

        public T Deserialize<T>(BinaryData input) => (T)(object)new NamedEvent(input.ToString());
    }

    private static (KurrentDBCatchupQueueCache Cache, KurrentDBQueueDataAdapter Adapter) CreateCache()
    {
        var typeMapper = new FakeTypeMapper();
        typeMapper.AddType(typeof(NamedEvent), EventType);
        var adapter = new KurrentDBQueueDataAdapter(OrleansSerializer, new KurrentDBDataAdapterOptions
        {
            ConnectionName = "test",
            ProviderName = "test",
            TypeMapper = typeMapper,
            GrainStorageSerializer = new NameSerializer(),
        });
        var logger = NullLogger.Instance;
        var bufferPool = new ObjectPool<FixedSizeBuffer>(() => new FixedSizeBuffer(1 << 16));
        var cursorTracker = new KurrentDBCursorTracker();
        var timePurge = new TimePurgePredicate(TimeSpan.FromHours(1), TimeSpan.FromHours(2));
        var evictionStrategy = new KurrentDBCatchupEvictionStrategy(logger, timePurge, null, null, adapter, cursorTracker);
        return (new KurrentDBCatchupQueueCache("queue", 100, bufferPool, adapter, evictionStrategy, cursorTracker, logger, null!, null, null), adapter);
    }

    private static StreamedEvent<KurrentDBRawEvent> CreateEvent(string name, string category, ulong commitPosition, TransactionInfo transaction,
        string[]? transactionScoped = null, string[]? eventScoped = null)
    {
        var stream = new StreamName($"{category}-1", category, null, "1");
        return new(Guid.NewGuid(), EventType, stream, (ulong)transaction.TransactionPartitionIndex,
            new StreamedEventCursor("test", stream.Category, stream.Value, commitPosition, 0), DateTime.UtcNow,
            new KurrentDBRawEvent(Encoding.UTF8.GetBytes(name)),
            EventMetadata.Create(null, null, null, transaction, null, transactionScoped, eventScoped));
    }

    private static string[] Delivered(KurrentDBCatchupQueueCache cache, StreamId streamId)
    {
        var cursor = cache.GetCursorAtPosition(streamId, StreamSubscriptionStartPosition.EarliestAvailable);
        try
        {
            var delivered = new List<string>();
            while (cache.TryGetNextMessage(cursor, out var container))
            {
                delivered.AddRange(container.GetEvents<IStreamedEvent<object>>().Select(e => ((NamedEvent)e.Item1.Event).Name));
            }
            return [.. delivered];
        }
        finally
        {
            cache.ReleaseCursor(cursor);
        }
    }

    [Fact]
    public void Each_Target_Receives_The_Events_Its_Scope_Covers()
    {
        var (cache, adapter) = CreateCache();
        var id = Guid.NewGuid();
        // A transaction over two streams: gen holds e0 and e2, other holds e1.
        cache.Add(
        [
            CreateEvent("e0", "gen", 100, new TransactionInfo(id, 3, 2, 0), transactionScoped: ["report/1"], eventScoped: ["order/a", "other/1"]),
            CreateEvent("e1", "other", 200, new TransactionInfo(id, 3, 1, 0), eventScoped: ["order/b"]),
            CreateEvent("e2", "gen", 300, new TransactionInfo(id, 3, 2, 1), eventScoped: ["order/a", "report/1"]),
        ], DateTime.UtcNow);

        Assert.Equal(["e0", "e2"], Delivered(cache, adapter.GetTargetStreamId("gen/1")));
        // A participant stream named as an event-scoped target receives that event merged with its own, in order.
        Assert.Equal(["e0", "e1"], Delivered(cache, adapter.GetTargetStreamId("other/1")));
        Assert.Equal(["e0", "e2"], Delivered(cache, adapter.GetTargetStreamId("order/a")));
        Assert.Equal(["e1"], Delivered(cache, adapter.GetTargetStreamId("order/b")));
        // A stream named in both scopes receives the whole transaction once.
        Assert.Equal(["e0", "e1", "e2"], Delivered(cache, adapter.GetTargetStreamId("report/1")));
    }
}
