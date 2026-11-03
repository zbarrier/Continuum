using Continuum.Orleans.Streams;

using Orleans.Runtime;
using Orleans.Streams;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies how a batch container hands its event to a consumer: only as a streamed event envelope, nothing when
///     the event was suppressed, and a fault on delivery when it could not be materialized.
/// </summary>
public class KurrentDBBatchContainerTests
{
    private sealed record TestEvent(string Value);

    // The container never reads the message data; the event it delivers comes from the materialization.
    private static KurrentDBMessage CreateMessage() =>
        new("test", StreamId.Create("test", "1"), 10, 10, Guid.NewGuid(), "test-event", Array.Empty<byte>(), new StreamName("test-1", "test", null, "1"));

    [Fact]
    public void Delivers_the_event_as_a_streamed_event()
    {
        IBatchContainer container = new KurrentDBBatchContainer(CreateMessage(), KurrentDBEventMaterialization.Resolved(new TestEvent("a")));

        var (streamedEvent, token) = Assert.Single(container.GetEvents<IStreamedEvent<object>>());

        Assert.Equal(new TestEvent("a"), streamedEvent.Event);
        Assert.Equal(container.SequenceToken, token);
    }

    [Fact]
    public void Delivers_to_a_consumer_subscribed_as_object()
    {
        IBatchContainer container = new KurrentDBBatchContainer(CreateMessage(), KurrentDBEventMaterialization.Resolved(new TestEvent("a")));

        var (item, _) = Assert.Single(container.GetEvents<object>());

        Assert.IsAssignableFrom<IStreamedEvent<object>>(item);
    }

    [Fact]
    public void Rejects_a_consumer_subscribed_as_the_domain_event_type()
    {
        IBatchContainer container = new KurrentDBBatchContainer(CreateMessage(), KurrentDBEventMaterialization.Resolved(new TestEvent("a")));

        var exception = Assert.Throws<InvalidOperationException>(() => container.GetEvents<TestEvent>());

        Assert.Contains(typeof(TestEvent).ToString(), exception.Message);
    }

    [Fact]
    public void Rejects_a_wrong_type_even_when_the_event_was_suppressed()
    {
        IBatchContainer container = new KurrentDBBatchContainer(CreateMessage(), KurrentDBEventMaterialization.Suppressed());

        Assert.Throws<InvalidOperationException>(() => container.GetEvents<string>());
    }

    [Fact]
    public void Delivers_nothing_for_a_suppressed_event()
    {
        IBatchContainer container = new KurrentDBBatchContainer(CreateMessage(), KurrentDBEventMaterialization.Suppressed());

        Assert.Empty(container.GetEvents<IStreamedEvent<object>>());
    }

    [Fact]
    public void Raises_the_fault_on_delivery_so_the_event_is_retried_rather_than_skipped()
    {
        IBatchContainer container = new KurrentDBBatchContainer(CreateMessage(), KurrentDBEventMaterialization.Faulted("Unknown event type."));

        var exception = Assert.Throws<InvalidOperationException>(() => container.GetEvents<IStreamedEvent<object>>());

        Assert.Equal("Unknown event type.", exception.Message);
    }

    [Fact]
    public void Raises_the_fault_on_every_delivery_attempt()
    {
        IBatchContainer container = new KurrentDBBatchContainer(CreateMessage(), KurrentDBEventMaterialization.Faulted("Unknown event type."));

        Assert.Throws<InvalidOperationException>(() => container.GetEvents<IStreamedEvent<object>>());
        Assert.Throws<InvalidOperationException>(() => container.GetEvents<IStreamedEvent<object>>());
    }
}
