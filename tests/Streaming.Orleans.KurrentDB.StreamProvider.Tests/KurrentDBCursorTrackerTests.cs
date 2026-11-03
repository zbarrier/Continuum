using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.CatchUp;

using Orleans.Runtime;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies the purge watermark: only reading cursors hold it back, caught-up cursors are woken per stream, and
///     released cursors never hold it again.
/// </summary>
public class KurrentDBCursorTrackerTests
{
    private static readonly StreamId StreamA = StreamId.Create("ns", "a");
    private static readonly StreamId StreamB = StreamId.Create("ns", "b");

    [Fact]
    public void No_Cursors_Leaves_Purging_Unrestricted()
    {
        var tracker = new KurrentDBCursorTracker();

        Assert.Null(tracker.GetSlowestCommitPosition());
    }

    [Fact]
    public void A_New_Cursor_Holds_The_Watermark_At_Zero()
    {
        var tracker = new KurrentDBCursorTracker();

        tracker.Register(StreamA, new object());

        Assert.Equal(0UL, tracker.GetSlowestCommitPosition());
    }

    [Fact]
    public void The_Watermark_Is_The_Slowest_Reading_Cursor()
    {
        var tracker = new KurrentDBCursorTracker();
        var fast = tracker.Register(StreamA, new object());
        var slow = tracker.Register(StreamB, new object());

        tracker.RecordProgress(fast, 300);
        tracker.RecordProgress(slow, 100);

        Assert.Equal(100UL, tracker.GetSlowestCommitPosition());
    }

    [Fact]
    public void A_Caught_Up_Cursor_Stops_Holding_The_Watermark()
    {
        var tracker = new KurrentDBCursorTracker();
        var caughtUp = tracker.Register(StreamA, new object());
        var reading = tracker.Register(StreamB, new object());
        tracker.RecordProgress(caughtUp, 100);
        tracker.RecordProgress(reading, 300);

        tracker.MarkCaughtUp(caughtUp);

        Assert.True(caughtUp.IsCaughtUp);
        Assert.Equal(300UL, tracker.GetSlowestCommitPosition());
    }

    [Fact]
    public void Marking_Caught_Up_Twice_Is_Harmless()
    {
        var tracker = new KurrentDBCursorTracker();
        var entry = tracker.Register(StreamA, new object());

        tracker.MarkCaughtUp(entry);
        tracker.MarkCaughtUp(entry);

        Assert.Single(tracker.Wake(StreamA, 10));
    }

    [Fact]
    public void Waking_Returns_Only_The_Streams_Caught_Up_Cursors_At_The_New_Position()
    {
        var tracker = new KurrentDBCursorTracker();
        var a = tracker.Register(StreamA, new object());
        var b = tracker.Register(StreamB, new object());
        tracker.MarkCaughtUp(a);
        tracker.MarkCaughtUp(b);

        var woken = tracker.Wake(StreamA, 500);

        Assert.Same(a, Assert.Single(woken));
        Assert.False(a.IsCaughtUp);
        Assert.Equal(500UL, a.CommitPosition);
        Assert.True(b.IsCaughtUp);
        Assert.Equal(500UL, tracker.GetSlowestCommitPosition());
    }

    [Fact]
    public void Waking_A_Stream_Without_Caught_Up_Cursors_Returns_Nothing()
    {
        var tracker = new KurrentDBCursorTracker();
        tracker.Register(StreamA, new object());

        Assert.Empty(tracker.Wake(StreamA, 10));
        Assert.Empty(tracker.Wake(StreamB, 10));
    }

    [Fact]
    public void Waking_Twice_Only_Returns_The_Cursors_Once()
    {
        var tracker = new KurrentDBCursorTracker();
        var entry = tracker.Register(StreamA, new object());
        tracker.MarkCaughtUp(entry);

        tracker.Wake(StreamA, 10);

        Assert.Empty(tracker.Wake(StreamA, 20));
        Assert.Equal(10UL, entry.CommitPosition);
    }

    [Fact]
    public void Unregistering_A_Reading_Cursor_Releases_The_Watermark()
    {
        var tracker = new KurrentDBCursorTracker();
        var entry = tracker.Register(StreamA, new object());

        tracker.Unregister(entry);

        Assert.True(entry.IsReleased);
        Assert.Null(tracker.GetSlowestCommitPosition());
    }

    [Fact]
    public void Unregistering_A_Caught_Up_Cursor_Keeps_It_From_Being_Woken()
    {
        var tracker = new KurrentDBCursorTracker();
        var entry = tracker.Register(StreamA, new object());
        tracker.MarkCaughtUp(entry);

        tracker.Unregister(entry);

        Assert.Empty(tracker.Wake(StreamA, 10));
        Assert.Null(tracker.GetSlowestCommitPosition());
    }

    [Fact]
    public void A_Released_Cursor_Ignores_Later_Calls()
    {
        var tracker = new KurrentDBCursorTracker();
        var entry = tracker.Register(StreamA, new object());
        tracker.RecordProgress(entry, 50);
        tracker.Unregister(entry);

        tracker.RecordProgress(entry, 100);
        tracker.MarkCaughtUp(entry);
        tracker.Unregister(entry);

        Assert.Equal(50UL, entry.CommitPosition);
        Assert.False(entry.IsCaughtUp);
        Assert.Empty(tracker.Wake(StreamA, 10));
        Assert.Null(tracker.GetSlowestCommitPosition());
    }

    [Fact]
    public void A_Purge_Uses_The_Watermark_From_When_It_Began()
    {
        var tracker = new KurrentDBCursorTracker();
        var entry = tracker.Register(StreamA, new object());
        tracker.RecordProgress(entry, 100);

        tracker.BeginPurge();
        tracker.RecordProgress(entry, 200);
        Assert.Equal(100UL, tracker.GetSlowestCommitPosition());

        tracker.EndPurge();
        Assert.Equal(200UL, tracker.GetSlowestCommitPosition());
    }

    [Fact]
    public void A_Purge_With_No_Reading_Cursors_Is_Unrestricted()
    {
        var tracker = new KurrentDBCursorTracker();

        tracker.BeginPurge();

        Assert.Null(tracker.GetSlowestCommitPosition());
    }
}
