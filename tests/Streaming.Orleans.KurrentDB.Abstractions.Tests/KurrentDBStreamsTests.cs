namespace Continuum.Streaming.Orleans.KurrentDB.Abstractions.Tests;

public class KurrentDBStreamsTests
{
    [Theory]
    [InlineData("$all")]
    [InlineData("$All")]
    [InlineData("$ALL")]
    public void Recognizes_The_All_Stream(string streamName)
    {
        Assert.True(KurrentDBStreams.IsAll(streamName));
    }

    [Theory]
    [InlineData("chat-1")]
    [InlineData("$all-chat")]
    public void Does_Not_Treat_Other_Streams_As_The_All_Stream(string streamName)
    {
        Assert.False(KurrentDBStreams.IsAll(streamName));
    }
}
