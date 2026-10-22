using Continuum.Streaming.Orleans.KurrentDB.Extensions;

namespace Continuum.Streaming.Orleans.KurrentDB.Abstractions.Tests;

public class StreamEventCursorMappingTests
{
    [Fact]
    public void Topic_Is_The_Category_And_Partition_Is_The_Stream_Key()
    {
        var cursor = KurrentDBStreamedEventConversions.ToCursor(new Continuum.Orleans.Streams.StreamName("chat-1", "chat", null, "1"), 42);

        Assert.Equal(new StreamedEventCursor("chat", "1", 42, 0), cursor);
    }

    [Fact]
    public void Partition_Includes_The_Tenant()
    {
        var cursor = KurrentDBStreamedEventConversions.ToCursor(new Continuum.Orleans.Streams.StreamName("chat-acme|1", "chat", "acme", "1"), 42);

        Assert.Equal("acme|1", cursor.Partition);
    }

    [Fact]
    public void Streams_In_The_Same_Category_Get_Different_Partitions()
    {
        var first = KurrentDBStreamedEventConversions.ToCursor(new Continuum.Orleans.Streams.StreamName("chat-1", "chat", null, "1"), 1);
        var second = KurrentDBStreamedEventConversions.ToCursor(new Continuum.Orleans.Streams.StreamName("chat-2", "chat", null, "2"), 2);

        Assert.Equal(first.Topic, second.Topic);
        Assert.NotEqual(first.Partition, second.Partition);
    }

    [Fact]
    public void All_Stream_Events_Share_One_Global_Cursor_Scope()
    {
        var first = ToAllStreamEvent("chat-1", 100);
        var second = ToAllStreamEvent("order-acme|2", 200);

        Assert.Equal(new StreamedEventCursor(KurrentDBStreamedEventConversions.AllStreamCheckpointTopic, KurrentDBStreamedEventConversions.DefaultPartitionId, 100, 0), first.Cursor);
        Assert.Equal(new StreamedEventCursor(KurrentDBStreamedEventConversions.AllStreamCheckpointTopic, KurrentDBStreamedEventConversions.DefaultPartitionId, 200, 0), second.Cursor);
    }

    private static StreamedEvent<object> ToAllStreamEvent(string streamId, ulong commitPosition)
    {
        var metadata = new Dictionary<string, string>
        {
            ["type"] = "test-event",
            ["created"] = DateTime.UtcNow.Ticks.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["content-type"] = "application/json",
        };
        var record = new global::KurrentDB.Client.EventRecord(
            streamId,
            global::KurrentDB.Client.Uuid.NewUuid(),
            new global::KurrentDB.Client.StreamPosition(0),
            new global::KurrentDB.Client.Position(commitPosition, commitPosition),
            metadata,
            ReadOnlyMemory<byte>.Empty,
            ReadOnlyMemory<byte>.Empty);
        return new global::KurrentDB.Client.ResolvedEvent(record, null, commitPosition).ToAllStreamEvent(new object());
    }
}
