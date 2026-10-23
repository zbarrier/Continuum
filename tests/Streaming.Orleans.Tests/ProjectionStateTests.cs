using System.Numerics;

using Orleans.Runtime;
using Continuum.Orleans.Streams;

namespace Continuum.Streaming.Orleans.Tests;

/// <summary>
///     Verifies the de-duplication contract of <see cref="ProjectionState{TState}" />.
/// </summary>
/// <remarks>
///     These are provider agnostic and run without a cluster, because the behaviour under test is the sequence
///     comparison rather than delivery. Redelivery cannot be forced through a real event store: appending the same
///     payload twice produces two distinct events at two distinct positions, which is genuinely two events. Handing
///     the same envelope to the state twice is what actually reproduces a redelivery after a failed write.
/// </remarks>
public class ProjectionStateTests
{
    private const string Category = "counted";
    private const string Topic = Category;
    private const string SourceConnection = "store";
    private const string StreamKey = "1";

    [Fact]
    public async Task Applies_An_Event_The_First_Time_It_Is_Seen()
    {
        var state = new CountingProjectionState();

        var hasChanges = await state.WhenAsync(Event(sequenceNumber: 1));

        Assert.True(hasChanges);
        Assert.Equal(1, state.Applied);
    }

    [Fact]
    public async Task Does_Not_Apply_The_Same_Event_Twice()
    {
        var state = new CountingProjectionState();
        var streamedEvent = Event(sequenceNumber: 1);

        await state.WhenAsync(streamedEvent);
        var hasChanges = await state.WhenAsync(streamedEvent);

        // The second delivery is a redelivery, not a second event. Reporting no changes also keeps the grain from
        // issuing a pointless state write.
        Assert.False(hasChanges);
        Assert.Equal(1, state.Applied);
    }

    [Fact]
    public async Task Does_Not_Apply_An_Event_Behind_The_Watermark()
    {
        var state = new CountingProjectionState();

        await state.WhenAsync(Event(sequenceNumber: 5));
        var hasChanges = await state.WhenAsync(Event(sequenceNumber: 4));

        Assert.False(hasChanges);
        Assert.Equal(1, state.Applied);
    }

    [Fact]
    public async Task Applies_Events_In_Ascending_Sequence()
    {
        var state = new CountingProjectionState();

        await state.WhenAsync(Event(sequenceNumber: 1));
        await state.WhenAsync(Event(sequenceNumber: 2));
        await state.WhenAsync(Event(sequenceNumber: 3));

        Assert.Equal(3, state.Applied);
    }

    [Fact]
    public async Task Distinguishes_Events_Sharing_A_Sequence_By_Sub_Sequence()
    {
        var state = new CountingProjectionState();

        await state.WhenAsync(Event(sequenceNumber: 1, subSequenceNumber: 0));
        var hasChanges = await state.WhenAsync(Event(sequenceNumber: 1, subSequenceNumber: 1));

        // Providers that report several events at one position rely on the sub sequence to stay ordered; without it
        // every event after the first at a given position would be discarded as already applied.
        Assert.True(hasChanges);
        Assert.Equal(2, state.Applied);
    }

    [Fact]
    public async Task Tracks_Partitions_Independently_Within_A_Stream()
    {
        var state = new CountingProjectionState();

        // After a repartition, a stream key can move to a partition whose sequence numbers are lower than those
        // already applied from its previous partition. A shared watermark would discard the new partition's events.
        await state.WhenAsync(Event(sequenceNumber: 10, partitionId: "shard-0"));
        var hasChanges = await state.WhenAsync(Event(sequenceNumber: 4, partitionId: "shard-1"));

        Assert.True(hasChanges);
        Assert.Equal(2, state.Applied);
    }

    [Fact]
    public async Task Does_Not_Reapply_An_Event_Within_The_Same_Partition()
    {
        var state = new CountingProjectionState();

        await state.WhenAsync(Event(sequenceNumber: 10, partitionId: "shard-0"));
        await state.WhenAsync(Event(sequenceNumber: 4, partitionId: "shard-1"));
        var hasChanges = await state.WhenAsync(Event(sequenceNumber: 10, partitionId: "shard-0"));

        Assert.False(hasChanges);
        Assert.Equal(2, state.Applied);
    }

    [Fact]
    public async Task Tracks_Topics_Independently()
    {
        var state = new CountingProjectionState();

        await state.WhenAsync(Event(sequenceNumber: 10, topic: "topic-a"));
        var hasChanges = await state.WhenAsync(Event(sequenceNumber: 4, topic: "topic-b"));

        Assert.True(hasChanges);
        Assert.Equal(2, state.Applied);
    }

    [Fact]
    public async Task Keeps_A_Separate_Watermark_Per_Source_Connection()
    {
        var state = new CountingProjectionState();

        // Positions are local to a database, so a lower position from another source is a different event.
        await state.WhenAsync(Event(sequenceNumber: 10, sourceConnectionName: "store-a"));
        var hasChanges = await state.WhenAsync(Event(sequenceNumber: 4, sourceConnectionName: "store-b"));

        Assert.True(hasChanges);
        Assert.Equal(2, state.Applied);
    }

    [Fact]
    public async Task Advances_The_Watermark_Even_When_The_Handler_Ignores_The_Event()
    {
        var state = new CountingProjectionState { Handled = false };

        var hasChanges = await state.WhenAsync(Event(sequenceNumber: 1));

        // An ignored event is still consumed, so it must not be reconsidered on redelivery.
        Assert.False(hasChanges);
        Assert.True(state.Watermarks.TryGet(SourceConnection, Topic, "0", out var lastSequence));
        Assert.Equal(new BigInteger(1), lastSequence.SequenceNumber);
    }

    [Fact]
    public async Task Shares_One_Watermark_Across_Streams_In_A_Partition()
    {
        var state = new CountingProjectionState();

        // Positions are ordered within a partition regardless of which stream an event belongs to, so an event
        // behind the partition's watermark has already been delivered, whichever stream it came from.
        await state.WhenAsync(Event(sequenceNumber: 10, streamKey: "b"));
        var hasChanges = await state.WhenAsync(Event(sequenceNumber: 4, streamKey: "a"));

        Assert.False(hasChanges);
        Assert.Equal(1, state.Applied);
    }

    [Fact]
    public async Task Applies_A_Batch_In_Order_And_Skips_Redelivered_Events()
    {
        var state = new CountingProjectionState();

        await state.WhenAsync(Event(sequenceNumber: 2));
        var hasChanges = await state.WhenAsync([Event(sequenceNumber: 1), Event(sequenceNumber: 2), Event(sequenceNumber: 3)]);

        // The batch path shares the watermark with single-event delivery, so only the event not yet seen is applied.
        Assert.True(hasChanges);
        Assert.Equal(2, state.Applied);
    }

    [Fact]
    public async Task Reports_No_Changes_For_A_Batch_Already_Applied()
    {
        var state = new CountingProjectionState();
        List<IStreamedEvent<object>> batch = [Event(sequenceNumber: 1), Event(sequenceNumber: 2)];

        await state.WhenAsync(batch);
        var hasChanges = await state.WhenAsync(batch);

        Assert.False(hasChanges);
        Assert.Equal(2, state.Applied);
    }

    [Fact]
    public void Records_A_Subscription_Once()
    {
        var state = new CountingProjectionState();

        Assert.True(state.AddSubscription(Source("a")));
        Assert.False(state.AddSubscription(Source("a")));

        Assert.Equal(new[] { Source("a") }, state.Subscriptions);
    }

    [Fact]
    public void Treats_The_Same_Stream_On_Another_Provider_As_A_Different_Subscription()
    {
        var state = new CountingProjectionState();

        state.AddSubscription(Source("a"));
        var added = state.AddSubscription(Source("a", provider: "other"));

        Assert.True(added);
        Assert.Equal(2, state.Subscriptions.Count);
    }

    [Fact]
    public void Removes_Only_The_Given_Subscription()
    {
        var state = new CountingProjectionState();
        state.AddSubscription(Source("a"));
        state.AddSubscription(Source("b"));

        Assert.True(state.RemoveSubscription(Source("a")));
        Assert.False(state.RemoveSubscription(Source("a")));

        Assert.Equal(new[] { Source("b") }, state.Subscriptions);
    }

    [Fact]
    public void Clears_Subscriptions_And_Reports_Whether_Any_Were_Removed()
    {
        var state = new CountingProjectionState();
        state.AddSubscription(Source("a"));
        state.AddSubscription(Source("b"));

        Assert.True(state.ClearSubscriptions());
        Assert.False(state.ClearSubscriptions());

        Assert.Empty(state.Subscriptions);
    }

    [Fact]
    public async Task Keeps_The_Watermark_After_A_Subscription_Is_Removed()
    {
        var state = new CountingProjectionState();
        state.AddSubscription(Source(StreamKey));
        await state.WhenAsync(Event(sequenceNumber: 1));

        state.RemoveSubscription(Source(StreamKey));
        state.AddSubscription(Source(StreamKey));
        var hasChanges = await state.WhenAsync(Event(sequenceNumber: 1));

        // Resubscribing may redeliver events already projected; they must not be applied again.
        Assert.False(hasChanges);
        Assert.Equal(1, state.Applied);
    }

    private static StreamSubscriptionSource Source(string key, string provider = "provider")
        => new(provider, StreamId.Create(Topic, key));

    private static StreamedEvent<object> Event(
        long sequenceNumber,
        ulong subSequenceNumber = 0,
        string partitionId = "0",
        string topic = Topic,
        string sourceConnectionName = SourceConnection,
        string category = Category,
        string? tenantId = null,
        string streamKey = StreamKey)
    {
        var rawName = tenantId is null ? $"{category}-{streamKey}" : $"{category}-{tenantId}|{streamKey}";
        return new StreamedEvent<object>(
            Guid.CreateVersion7(),
            "Tests.Counted",
            streamName: new StreamName(rawName, category, tenantId, streamKey),
            streamVersion: 0,
            cursor: new StreamedEventCursor(sourceConnectionName, topic, partitionId, new BigInteger(sequenceNumber), subSequenceNumber),
            timestamp: DateTime.UtcNow,
            @event: new object());
    }

    private sealed class CountingProjectionState : ProjectionState<CountingProjectionState>
    {
        public int Applied { get; private set; }

        public bool Handled { get; init; } = true;

        protected override ValueTask<bool> Handle(IStreamedEvent<object> streamedEvent)
        {
            Applied++;
            return ValueTask.FromResult(Handled);
        }
    }
}
