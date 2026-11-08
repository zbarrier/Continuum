using Continuum.Orleans.Streams;

using Orleans.Providers.Streams.Common;
using Orleans.Runtime;
using Orleans.Streams;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies that a batch holding a transaction is delivered whole or not at all, so no event of it can be skipped.
/// </summary>
public class KurrentDBTransactionBatchContainerTests
{
    private sealed record TestEvent(string Value);

    private static KurrentDBMessage CreateMessage(ulong commitPosition) =>
        new("test", StreamId.Create("test", "1"), commitPosition, (long)commitPosition, Guid.NewGuid(), "test-event", Array.Empty<byte>(), new StreamName("test-1", "test", null, "1"));

    [Fact]
    public void Delivers_every_event_in_order_with_its_own_rising_token_and_the_batch_at_the_last_event()
    {
        IBatchContainer container = new KurrentDBBatchContainer(
            [CreateMessage(10), CreateMessage(11), CreateMessage(12)],
            [KurrentDBEventMaterialization.Resolved(new TestEvent("a")), KurrentDBEventMaterialization.Resolved(new TestEvent("b")), KurrentDBEventMaterialization.Resolved(new TestEvent("c"))]);

        var events = container.GetEvents<IStreamedEvent<object>>().ToList();

        Assert.Equal([new TestEvent("a"), new TestEvent("b"), new TestEvent("c")], events.Select(e => e.Item1.Event));
        Assert.Equal([new EventSequenceToken(10, 0), new EventSequenceToken(11, 0), new EventSequenceToken(12, 0)], events.Select(e => (EventSequenceToken)e.Item2));
        Assert.Equal(new EventSequenceToken(12, 0), container.SequenceToken);
        Assert.Equal(12UL, ((IKurrentDBCommitPositionBatch)container).CommitPosition);
    }

    [Fact]
    public void Faults_the_whole_transaction_when_an_earlier_event_could_not_be_materialized()
    {
        IBatchContainer container = new KurrentDBBatchContainer(
            [CreateMessage(10), CreateMessage(11)],
            [KurrentDBEventMaterialization.Faulted("Unknown event type."), KurrentDBEventMaterialization.Resolved(new TestEvent("b"))]);

        var exception = Assert.Throws<InvalidOperationException>(() => container.GetEvents<IStreamedEvent<object>>());

        Assert.Equal("Unknown event type.", exception.Message);
    }

    [Fact]
    public void Leaves_out_only_the_suppressed_events()
    {
        IBatchContainer container = new KurrentDBBatchContainer(
            [CreateMessage(10), CreateMessage(11)],
            [KurrentDBEventMaterialization.Suppressed(), KurrentDBEventMaterialization.Resolved(new TestEvent("b"))]);

        var (streamedEvent, _) = Assert.Single(container.GetEvents<IStreamedEvent<object>>());

        Assert.Equal(new TestEvent("b"), streamedEvent.Event);
    }
}
