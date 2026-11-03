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
///     Verifies that cache pressure is gated on the number of cached events, not on the spread of their commit
///     positions, which are byte offsets in the log.
/// </summary>
public class KurrentDBCatchUpQueueCachePressureTests
{
    // Pressure is only measured once the cache holds 10 * maxAddCount events.
    private const int MaxAddCount = 1;
    private const int MinimumItems = 10 * MaxAddCount;

    // A realistic gap between consecutive $all commit positions, in bytes.
    private const ulong PositionGap = 1_000;

    private static readonly Serializer OrleansSerializer = new ServiceCollection().AddSerializer().BuildServiceProvider().GetRequiredService<Serializer>();

    private static readonly StreamName Stream = new("test-1", "test", null, "1");

    private sealed class FakeTypeMapper : TypeMapper;

    private sealed class FakeSerializer : IGrainStorageSerializer
    {
        public BinaryData Serialize<T>(T? input) => throw new NotSupportedException();

        public T Deserialize<T>(BinaryData input) => throw new NotSupportedException();
    }

    private sealed class RecordingMonitor : ICachePressureMonitor
    {
        public List<double> Contributions { get; } = [];

        public ICacheMonitor CacheMonitor { set { } }

        public void RecordCachePressureContribution(double cachePressureContribution) => Contributions.Add(cachePressureContribution);

        public bool IsUnderPressure(DateTime utcNow) => false;
    }

    private static (KurrentDBCatchUpQueueCache Cache, RecordingMonitor Monitor) CreateCache()
    {
        var adapter = new KurrentDBQueueDataAdapter(OrleansSerializer, new KurrentDBDataAdapterOptions
        {
            ConnectionName = "test",
            ProviderName = "test",
            TypeMapper = new FakeTypeMapper(),
            GrainStorageSerializer = new FakeSerializer(),
        });
        var logger = NullLogger.Instance;
        var bufferPool = new ObjectPool<FixedSizeBuffer>(() => new FixedSizeBuffer(1 << 20));
        var cursorTracker = new KurrentDBCursorTracker();
        var timePurge = new TimePurgePredicate(TimeSpan.FromHours(1), TimeSpan.FromHours(2));
        var evictionStrategy = new KurrentDBCatchUpEvictionStrategy(logger, timePurge, null, null, adapter, cursorTracker);
        var cache = new KurrentDBCatchUpQueueCache("queue", MaxAddCount, bufferPool, adapter, evictionStrategy, cursorTracker, logger, null!, null, null);
        var monitor = new RecordingMonitor();
        cache.AddCachePressureMonitor(monitor);
        return (cache, monitor);
    }

    private static List<StreamedEvent<KurrentDBRawEvent>> CreateEvents(int count) =>
        Enumerable.Range(0, count)
            .Select(i => new StreamedEvent<KurrentDBRawEvent>(Guid.NewGuid(), "test-event", Stream, (ulong)i,
                new StreamedEventCursor("test", Stream.Category, Stream.Value, (ulong)(i + 1) * PositionGap, 0), DateTime.UtcNow, new KurrentDBRawEvent([1])))
            .ToList();

    private static List<double> ReadFirst(KurrentDBCatchUpQueueCache cache, RecordingMonitor monitor, StreamId streamId)
    {
        var cursor = cache.GetCursorAtPosition(streamId, StreamSubscriptionStartPosition.EarliestAvailable);
        Assert.True(cache.TryGetNextMessage(cursor, out _));
        return monitor.Contributions;
    }

    [Fact]
    public void A_Few_Events_Far_Apart_In_The_Log_Do_Not_Measure_Pressure()
    {
        // The commit position spread is far above the item threshold, but there are too few events to measure.
        var (cache, monitor) = CreateCache();
        var positions = cache.Add(CreateEvents(MinimumItems - 1), DateTime.UtcNow);

        var contributions = ReadFirst(cache, monitor, positions[0].StreamId);

        Assert.Equal([0.0], contributions);
    }

    [Fact]
    public void Enough_Events_Measure_Pressure_From_The_Cursor_Position()
    {
        var (cache, monitor) = CreateCache();
        var positions = cache.Add(CreateEvents(MinimumItems + 1), DateTime.UtcNow);

        var contributions = ReadFirst(cache, monitor, positions[0].StreamId);

        // The oldest event is the furthest from the newest, so it is the most at risk.
        Assert.Equal([1.0], contributions);
    }
}
