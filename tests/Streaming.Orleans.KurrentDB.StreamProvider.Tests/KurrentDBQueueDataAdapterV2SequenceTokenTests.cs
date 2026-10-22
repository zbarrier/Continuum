using System.Numerics;

using Continuum.Streaming.Orleans.KurrentDB.StreamProvider;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

public class KurrentDBQueueDataAdapterV2SequenceTokenTests
{
    [Fact]
    public void Converts_The_Largest_Representable_Sequence_Number()
    {
        Assert.Equal(long.MaxValue, KurrentDBQueueDataAdapterV2.ToOrleansSequenceToken(new BigInteger(long.MaxValue), "chat-1"));
    }

    [Fact]
    public void Rejects_A_Commit_Position_Past_The_Orleans_Token_Range()
    {
        // A KurrentDB commit position is a ulong, so the top half of its range is unrepresentable as a signed token.
        var justPastLongRange = new BigInteger(long.MaxValue) + 1;

        var exception = Assert.Throws<NotSupportedException>(
            () => KurrentDBQueueDataAdapterV2.ToOrleansSequenceToken(justPastLongRange, "chat-1"));

        Assert.Contains("chat-1", exception.Message, StringComparison.Ordinal);
    }
}
