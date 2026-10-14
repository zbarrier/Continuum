using Continuum.Streaming.Orleans.KurrentDB.Extensions;

namespace Continuum.Streaming.Orleans.KurrentDB.Tests;

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
}
