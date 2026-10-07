using System.Numerics;

using Orleans.Runtime;

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
    private const string Topic = "counted";
    private const string StreamName = "counted-1";
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
    public async Task Ignores_The_Partition_When_Tracking_A_Stream()
    {
        var state = new CountingProjectionState();

        // A topic and key already name one stream, and a sequence is ordered within it. If the same stream were ever
        // reported under a different partition, keying by partition would split its watermark and let an event that
        // was already applied be applied a second time.
        await state.WhenAsync(Event(sequenceNumber: 10, partitionId: "shard-0"));
        var hasChanges = await state.WhenAsync(Event(sequenceNumber: 4, partitionId: "shard-1"));

        Assert.False(hasChanges);
        Assert.Equal(1, state.Applied);
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
    public async Task Advances_The_Watermark_Even_When_The_Handler_Ignores_The_Event()
    {
        var state = new CountingProjectionState { Handled = false };

        var hasChanges = await state.WhenAsync(Event(sequenceNumber: 1));

        // An ignored event is still consumed, so it must not be reconsidered on redelivery.
        Assert.False(hasChanges);
        var lastSequence = state.LastSequenceByTopicThenStreamKey[Topic][StreamKey];
        Assert.Equal(new BigInteger(1), lastSequence.SequenceNumber);
    }

    [Fact]
    public async Task Tracks_Streams_Independently_Within_A_Topic()
    {
        var state = new CountingProjectionState();

        // The shape a grain sees when it consumes several streams through one $all subscription: the topic is the
        // shared category and the sequence is a global log position, so the stream that is further ahead arrives
        // with the higher number regardless of which stream produced it.
        await state.WhenAsync(Event(sequenceNumber: 10, streamKey: "b"));
        var hasChanges = await state.WhenAsync(Event(sequenceNumber: 4, streamKey: "a"));

        // Keyed only by topic this event is behind the watermark and is silently dropped.
        Assert.True(hasChanges);
        Assert.Equal(2, state.Applied);
    }

    [Fact]
    public async Task Separates_Streams_Sharing_A_Key_Across_Topics()
    {
        var state = new CountingProjectionState();

        // A key is only unique within its category, so "order/1" and "snack/1" are different streams and must not
        // share a watermark.
        await state.WhenAsync(Event(sequenceNumber: 10, topic: "order", streamKey: "1"));
        var hasChanges = await state.WhenAsync(Event(sequenceNumber: 4, topic: "snack", streamKey: "1"));

        Assert.True(hasChanges);
        Assert.Equal(2, state.Applied);
    }

    [Fact]
    public async Task Tracks_A_Stream_By_Identity_Rather_Than_By_Its_Raw_Name()
    {
        var state = new CountingProjectionState();

        // The watermark is keyed by the identity the store groups by, not by the raw name it arrived under, so two
        // names that decompose to the same topic and key are one stream and keep one watermark.
        await state.WhenAsync(Event(sequenceNumber: 10, streamName: "counted-1"));
        var hasChanges = await state.WhenAsync(Event(sequenceNumber: 4, streamName: "some/other/form/counted-1"));

        Assert.False(hasChanges);
        Assert.Equal(1, state.Applied);
    }

    [Fact]
    public async Task Still_Discards_A_Redelivery_On_The_Same_Stream()
    {
        var state = new CountingProjectionState();

        // Adding the stream key to the watermark must not weaken de-duplication within a stream, which is the
        // guarantee that makes redelivery after a failed write safe.
        await state.WhenAsync(Event(sequenceNumber: 10, streamKey: "a"));
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
        string streamName = StreamName,
        string streamKey = StreamKey)
    {
        return new StreamedEvent<object>(
            NewId.Next(),
            "Tests.Counted",
            streamName: streamName,
            streamKey: streamKey,
            topic: topic,
            partitionId: partitionId,
            streamVersion: 0,
            streamPosition: 0,
            sequenceNumber: new BigInteger(sequenceNumber),
            subSequenceNumber: subSequenceNumber,
            timestamp: DateTime.UtcNow,
            evt: new object());
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
