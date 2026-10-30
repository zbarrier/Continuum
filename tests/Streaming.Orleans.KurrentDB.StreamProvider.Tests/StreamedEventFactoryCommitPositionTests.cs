using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.CatchUp;
using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Persistent;

using KurrentDB.Client;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies which commit position each strategy's event factory puts on the carrier, in particular that a resolved
///     link carries the position it was read at in <c>$all</c> rather than the position of the event it points to.
/// </summary>
public class StreamedEventFactoryCommitPositionTests
{
    private const string Connection = "conn";
    private const ulong EventCommitPosition = 1_000;
    private const ulong LinkCommitPosition = 9_000;

    private static EventRecord CreateRecord(string streamName, ulong streamVersion, ulong commitPosition, string eventType = "test-event") =>
        new(streamName,
            Uuid.NewUuid(),
            new StreamPosition(streamVersion),
            new Position(commitPosition, commitPosition),
            new Dictionary<string, string>
            {
                ["type"] = eventType,
                ["created"] = DateTime.UtcNow.Ticks.ToString(),
                ["content-type"] = "application/json",
            },
            new byte[] { 1, 2, 3 },
            ReadOnlyMemory<byte>.Empty);

    private static ResolvedEvent CreateEvent() =>
        new(CreateRecord("order-1", 4, EventCommitPosition), null, EventCommitPosition);

    // A link from a projection stream to the order event. The subscription read the link at LinkCommitPosition.
    private static ResolvedEvent CreateResolvedLink() =>
        new(CreateRecord("order-1", 4, EventCommitPosition), CreateRecord("$ce-order", 0, LinkCommitPosition, "$>"), LinkCommitPosition);

    [Fact]
    public void A_Catch_Up_Event_Carries_Its_Commit_Position()
    {
        var resolved = CreateEvent();

        var carrier = KurrentDBCatchUpStreamedEventFactory.FromAllStream(resolved, Connection);

        Assert.Equal(resolved.Event.EventId.ToGuid(), carrier.EventId);
        Assert.Equal("test-event", carrier.EventType);
        Assert.Equal(4UL, carrier.StreamVersion);
        Assert.Equal(Connection, carrier.Cursor.SourceConnectionName);
        Assert.Equal("order", carrier.Cursor.Topic);
        Assert.Equal("1", carrier.Cursor.Partition);
        Assert.Equal(EventCommitPosition, carrier.Cursor.SequenceNumber);
    }

    [Fact]
    public void A_Catch_Up_Resolved_Link_Carries_The_Position_It_Was_Read_At()
    {
        var carrier = KurrentDBCatchUpStreamedEventFactory.FromAllStream(CreateResolvedLink(), Connection);

        Assert.Equal(LinkCommitPosition, carrier.Cursor.SequenceNumber);
        // The rest of the carrier describes the linked event.
        Assert.Equal("order", carrier.Cursor.Topic);
        Assert.Equal(4UL, carrier.StreamVersion);
    }

    [Fact]
    public void A_Catch_Up_Event_Without_An_Original_Position_Falls_Back_To_The_Events_Position()
    {
        var resolved = new ResolvedEvent(CreateRecord("order-1", 4, EventCommitPosition), null, null);

        var carrier = KurrentDBCatchUpStreamedEventFactory.FromAllStream(resolved, Connection);

        Assert.Equal(EventCommitPosition, carrier.Cursor.SequenceNumber);
    }

    [Fact]
    public void A_Persistent_Event_Carries_Its_Commit_Position()
    {
        var resolved = CreateEvent();

        var carrier = KurrentDBPersistentStreamedEventFactory.FromPersistentSubscription(resolved, Connection);

        Assert.Equal(resolved.Event.EventId.ToGuid(), carrier.EventId);
        Assert.Equal(Connection, carrier.Cursor.SourceConnectionName);
        Assert.Equal("order", carrier.Cursor.Topic);
        Assert.Equal(EventCommitPosition, carrier.Cursor.SequenceNumber);
    }

    [Fact]
    public void A_Persistent_Resolved_Link_Carries_The_Position_It_Was_Read_At()
    {
        var carrier = KurrentDBPersistentStreamedEventFactory.FromPersistentSubscription(CreateResolvedLink(), Connection);

        Assert.Equal(LinkCommitPosition, carrier.Cursor.SequenceNumber);
        Assert.Equal("order", carrier.Cursor.Topic);
    }

    [Fact]
    public void A_Persistent_Event_Without_An_Original_Position_Falls_Back_To_The_Events_Position()
    {
        var resolved = new ResolvedEvent(CreateRecord("order-1", 4, EventCommitPosition), null, null);

        var carrier = KurrentDBPersistentStreamedEventFactory.FromPersistentSubscription(resolved, Connection);

        Assert.Equal(EventCommitPosition, carrier.Cursor.SequenceNumber);
    }
}
