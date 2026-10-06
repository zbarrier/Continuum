using System.Numerics;

namespace Continuum.Streaming.Tests;

/// <summary>
///     Covers the points where a <see cref="BigInteger" /> sequence number has to be narrowed for a transport that
///     uses a fixed-width integer.
/// </summary>
public class StreamSequenceNumberTests
{
    [Fact]
    public void Converts_A_Sequence_Number_Within_Range_To_An_Orleans_Token()
    {
        var result = StreamSequenceNumber.ToOrleansSequenceToken(new BigInteger(12796), "chat-1");

        Assert.Equal(12796L, result);
    }

    [Fact]
    public void Converts_The_Largest_Representable_Sequence_Number_To_An_Orleans_Token()
    {
        var result = StreamSequenceNumber.ToOrleansSequenceToken(new BigInteger(long.MaxValue), "chat-1");

        Assert.Equal(long.MaxValue, result);
    }

    [Fact]
    public void Rejects_A_Sequence_Number_Too_Wide_For_An_Orleans_Token()
    {
        // A Kinesis sequence number is a decimal far wider than 64 bits, which is the case this guard exists for.
        var kinesisStyleSequenceNumber = BigInteger.Parse("49590338271490256608559692538361571095921575989136588898");

        var exception = Assert.Throws<NotSupportedException>(
            () => StreamSequenceNumber.ToOrleansSequenceToken(kinesisStyleSequenceNumber, "orders/shard-0"));

        // The message has to name the stream and the limit, because the alternative it replaces is a bare
        // OverflowException raised from a cast with no context.
        Assert.Contains("orders/shard-0", exception.Message, StringComparison.Ordinal);
        Assert.Contains(kinesisStyleSequenceNumber.ToString(), exception.Message, StringComparison.Ordinal);
        Assert.Contains("catch-up subscription", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Rejects_A_Sequence_Number_Just_Past_The_Orleans_Token_Range()
    {
        // A KurrentDB commit position is a ulong, so the top half of its range is unrepresentable as a signed token.
        var justPastLongRange = new BigInteger(long.MaxValue) + 1;

        Assert.Throws<NotSupportedException>(
            () => StreamSequenceNumber.ToOrleansSequenceToken(justPastLongRange, "chat-1"));
    }

    [Fact]
    public void Converts_The_Largest_Representable_Commit_Position()
    {
        var result = StreamSequenceNumber.ToCommitPosition(new BigInteger(ulong.MaxValue), "chat-1");

        Assert.Equal(ulong.MaxValue, result);
    }

    [Fact]
    public void Rejects_A_Sequence_Number_Too_Wide_For_A_Commit_Position()
    {
        var justPastULongRange = new BigInteger(ulong.MaxValue) + 1;

        var exception = Assert.Throws<NotSupportedException>(
            () => StreamSequenceNumber.ToCommitPosition(justPastULongRange, "chat-1"));

        Assert.Contains("chat-1", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Rejects_A_Negative_Sequence_Number_As_A_Commit_Position()
    {
        var exception = Assert.Throws<NotSupportedException>(
            () => StreamSequenceNumber.ToCommitPosition(BigInteger.MinusOne, "chat-1"));

        Assert.Contains("chat-1", exception.Message, StringComparison.Ordinal);
    }
}
