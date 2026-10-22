using System.Numerics;

namespace Continuum.Streaming.Orleans.KurrentDB.Abstractions.Tests;

public class StreamedEventCursorExtensionsTests
{
    [Fact]
    public void Converts_The_Largest_Representable_Position()
    {
        var cursor = new StreamedEventCursor("orders", "1", new BigInteger(ulong.MaxValue), 0);

        Assert.Equal(ulong.MaxValue, cursor.ToKurrentDBPosition("chat-1"));
    }

    [Fact]
    public void Rejects_A_Sequence_Number_Too_Wide_For_A_Position()
    {
        var cursor = new StreamedEventCursor("orders", "1", new BigInteger(ulong.MaxValue) + 1, 0);

        var exception = Assert.Throws<NotSupportedException>(() => cursor.ToKurrentDBPosition("chat-1"));

        Assert.Contains("chat-1", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Rejects_A_Negative_Sequence_Number()
    {
        var cursor = new StreamedEventCursor("orders", "1", BigInteger.MinusOne, 0);

        var exception = Assert.Throws<NotSupportedException>(() => cursor.ToKurrentDBPosition("chat-1"));

        Assert.Contains("chat-1", exception.Message, StringComparison.Ordinal);
    }
}
