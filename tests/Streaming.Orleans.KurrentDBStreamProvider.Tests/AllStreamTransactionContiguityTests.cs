using System.Text;

using KurrentDB.Client;

namespace Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests;

/// <summary>
///     Establishes whether the events of a single atomic append stay contiguous in <c>$all</c> when other writers
///     append concurrently.
/// </summary>
/// <remarks>
///     The transaction-aware delivery design rests on an assumption about <c>$all</c> layout that has never been
///     verified against a real server. If a single append is always written as one contiguous run, a consumer reading
///     <c>$all</c> in order never sees a transaction split by unrelated events. If it is not, then any consumer reading
///     more than one stream can observe a transaction interleaved with other writers' events, and no amount of
///     consumer-side ordering fixes that without buffering.
///
///     These tests do not try to win a race. They run many rounds of concurrent appends released from a shared barrier
///     and report the worst interleaving observed across all of them, so a single positive observation is decisive
///     while a negative result is only evidence for this server version under this level of contention.
/// </remarks>
[Collection(ClusterCollection.Name)]
public class AllStreamTransactionContiguityTests
{
    private const string ConnectionString = "kurrentdb://localhost:2113?tls=false";
    private const string EventType = "contiguity-probe-event";

    /// <summary>The number of writers appending at the same time, mirroring several grains committing at once.</summary>
    private const int WriterCount = 32;

    /// <summary>The number of events each writer appends in one atomic call.</summary>
    private const int EventsPerAppend = 20;

    /// <summary>How many times the concurrent append is repeated, to give interleaving a chance to appear.</summary>
    private const int Rounds = 50;

    /// <summary>The number of streams a single multi-stream atomic append writes to.</summary>
    private const int StreamsPerMultiAppend = 3;

    private readonly ITestOutputHelper _output;

    public AllStreamTransactionContiguityTests(ClusterFixture fixture, ITestOutputHelper output)
    {
        // The fixture is depended upon so the KurrentDB container is started, even though the cluster is not used.
        ArgumentNullException.ThrowIfNull(fixture);
        _output = output;
    }

    [Fact]
    public async Task Reports_Whether_A_Single_Append_Stays_Contiguous_In_The_All_Stream()
    {
        // Arrange
        await using var client = new KurrentDBClient(KurrentDBClientSettings.Create(ConnectionString));
        var probeId = $"contiguity{Guid.NewGuid():N}";
        var startPosition = await GetCurrentEndPositionAsync(client).ConfigureAwait(false);

        // Act
        for (var round = 0; round < Rounds; round++)
        {
            await AppendConcurrentlyAsync(client, probeId, round).ConfigureAwait(false);
        }

        var log = await ReadProbeEventsAsync(client, probeId, startPosition).ConfigureAwait(false);

        // Assert
        Assert.Equal(Rounds * WriterCount * EventsPerAppend, log.Count);

        var splitAppends = 0;
        var worstGap = 0;
        string? worstAppend = null;

        foreach (var group in log.GroupBy(entry => entry.AppendKey))
        {
            var indexes = group.Select(entry => entry.LogIndex).Order().ToList();
            // A contiguous append occupies a run of consecutive slots in the filtered log, so the span across its
            // first and last event equals its event count. Anything wider means foreign events landed inside it.
            var span = indexes[^1] - indexes[0] + 1;
            var gap = span - indexes.Count;
            if (gap <= 0)
            {
                continue;
            }
            splitAppends++;
            if (gap > worstGap)
            {
                worstGap = gap;
                worstAppend = group.Key;
            }
        }

        _output.WriteLine($"Rounds: {Rounds}, writers per round: {WriterCount}, events per append: {EventsPerAppend}.");
        _output.WriteLine($"Appends observed: {log.Select(entry => entry.AppendKey).Distinct().Count()}.");
        _output.WriteLine($"Appends split by foreign events: {splitAppends}.");
        if (worstAppend is not null)
        {
            _output.WriteLine($"Worst append: {worstAppend} had {worstGap} foreign event(s) interleaved.");
            _output.WriteLine("CONCLUSION: a single atomic append is NOT contiguous in $all under concurrency.");
        }
        else
        {
            _output.WriteLine("CONCLUSION: every atomic append was contiguous in $all in this run.");
        }

        // The result is reported rather than asserted, because either outcome is a legitimate finding about the
        // server and the point of this test is to record which one holds.
        Assert.True(splitAppends >= 0);
    }

    /// <summary>
    ///     The same probe, but for a single atomic append that spans several streams.
    /// </summary>
    /// <remarks>
    ///     This is the case that actually matters for a consumer subscribing to more than one stream. If a
    ///     multi-stream commit is contiguous in <c>$all</c>, then such a consumer never observes one commit's events
    ///     split by another writer's, and consumer-side transaction handling is only ever needed for grouping rather
    ///     than for repairing order.
    /// </remarks>
    [Fact]
    public async Task Reports_Whether_A_Multi_Stream_Append_Stays_Contiguous_In_The_All_Stream()
    {
        // Arrange
        await using var client = new KurrentDBClient(KurrentDBClientSettings.Create(ConnectionString));
        var probeId = $"multicontiguity{Guid.NewGuid():N}";
        var startPosition = await GetCurrentEndPositionAsync(client).ConfigureAwait(false);

        // Act
        for (var round = 0; round < Rounds; round++)
        {
            await MultiStreamAppendConcurrentlyAsync(client, probeId, round).ConfigureAwait(false);
        }

        var log = await ReadProbeEventsAsync(client, probeId, startPosition).ConfigureAwait(false);

        // Assert
        Assert.Equal(Rounds * WriterCount * StreamsPerMultiAppend * EventsPerAppend, log.Count);

        var splitAppends = 0;
        var worstGap = 0;
        string? worstAppend = null;

        foreach (var group in log.GroupBy(entry => entry.AppendKey))
        {
            var indexes = group.Select(entry => entry.LogIndex).Order().ToList();
            var span = indexes[^1] - indexes[0] + 1;
            var gap = span - indexes.Count;
            if (gap <= 0)
            {
                continue;
            }
            splitAppends++;
            if (gap > worstGap)
            {
                worstGap = gap;
                worstAppend = group.Key;
            }
        }

        _output.WriteLine($"Rounds: {Rounds}, writers per round: {WriterCount}, streams per append: {StreamsPerMultiAppend}, events per stream: {EventsPerAppend}.");
        _output.WriteLine($"Multi-stream appends observed: {log.Select(entry => entry.AppendKey).Distinct().Count()}.");
        _output.WriteLine($"Multi-stream appends split by foreign events: {splitAppends}.");
        if (worstAppend is not null)
        {
            _output.WriteLine($"Worst append: {worstAppend} had {worstGap} foreign event(s) interleaved.");
            _output.WriteLine("CONCLUSION: a multi-stream atomic append is NOT contiguous in $all under concurrency.");
        }
        else
        {
            _output.WriteLine("CONCLUSION: every multi-stream atomic append was contiguous in $all in this run.");
        }

        Assert.True(splitAppends >= 0);
    }

    /// <summary>
    ///     Issues concurrent multi-stream atomic appends, each writing to several streams in one commit.
    /// </summary>
    private static async Task MultiStreamAppendConcurrentlyAsync(KurrentDBClient client, string probeId, int round)
    {
        using var barrier = new SemaphoreSlim(0, WriterCount);
        var writers = Enumerable.Range(0, WriterCount).Select(async writer =>
        {
            await barrier.WaitAsync().ConfigureAwait(false);
            var appendKey = $"r{round}w{writer}";
            var requests = Enumerable.Range(0, StreamsPerMultiAppend).Select(stream =>
            {
                var streamName = $"{probeId}-w{writer}s{stream}";
                var records = Enumerable.Range(0, EventsPerAppend).Select(sequence =>
                    new EventData(Uuid.NewUuid(), EventType, Encoding.UTF8.GetBytes($"{appendKey}:{stream}-{sequence}")));
                return new AppendStreamRequest(streamName, StreamState.Any, records);
            });
            await client.MultiStreamAppendAsync(requests).ConfigureAwait(false);
        }).ToList();

        barrier.Release(WriterCount);
        await Task.WhenAll(writers).ConfigureAwait(false);
    }

    /// <summary>
    ///     Releases every writer from a shared barrier so the appends overlap as tightly as the client allows.
    /// </summary>
    private static async Task AppendConcurrentlyAsync(KurrentDBClient client, string probeId, int round)
    {
        using var barrier = new SemaphoreSlim(0, WriterCount);
        var writers = Enumerable.Range(0, WriterCount).Select(async writer =>
        {
            await barrier.WaitAsync().ConfigureAwait(false);
            var appendKey = $"r{round}w{writer}";
            var streamName = $"{probeId}-writer{writer}";
            var events = Enumerable.Range(0, EventsPerAppend).Select(sequence =>
                new EventData(Uuid.NewUuid(), EventType, Encoding.UTF8.GetBytes($"{appendKey}:{sequence}")));
            await client.AppendToStreamAsync(streamName, StreamState.Any, events).ConfigureAwait(false);
        }).ToList();

        barrier.Release(WriterCount);
        await Task.WhenAll(writers).ConfigureAwait(false);
    }

    private static async Task<Position> GetCurrentEndPositionAsync(KurrentDBClient client)
    {
        var last = client.ReadAllAsync(Direction.Backwards, Position.End, maxCount: 1);
        await foreach (var resolved in last.ConfigureAwait(false))
        {
            return resolved.OriginalPosition ?? Position.Start;
        }
        return Position.Start;
    }

    /// <summary>
    ///     Reads <c>$all</c> forward from the probe's start position, keeping only this probe's events but preserving
    ///     their relative order, so the index of each event reflects its position in the global log.
    /// </summary>
    private static async Task<List<(string AppendKey, int LogIndex)>> ReadProbeEventsAsync(KurrentDBClient client, string probeId, Position startPosition)
    {
        var entries = new List<(string AppendKey, int LogIndex)>();
        var index = 0;
        await foreach (var resolved in client.ReadAllAsync(Direction.Forwards, startPosition).ConfigureAwait(false))
        {
            if (resolved.Event.EventType != EventType)
            {
                continue;
            }
            if (resolved.Event.EventStreamId.StartsWith(probeId, StringComparison.Ordinal) == false)
            {
                continue;
            }
            var payload = Encoding.UTF8.GetString(resolved.Event.Data.Span);
            var appendKey = payload[..payload.IndexOf(':', StringComparison.Ordinal)];
            entries.Add((appendKey, index));
            index++;
        }
        return entries;
    }
}
