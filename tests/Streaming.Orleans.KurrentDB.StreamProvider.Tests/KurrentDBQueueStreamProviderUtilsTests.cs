using Continuum.Streaming.Orleans.KurrentDB.StreamProvider;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies the format and number of the default queue names generated for a stream provider.
/// </summary>
public class KurrentDBQueueStreamProviderUtilsTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(16)]
    public void Generates_One_Name_Per_Queue(int totalQueueCount)
    {
        var names = KurrentDBQueueStreamProviderUtils.GenerateDefaultQueueNames("service", "provider", totalQueueCount);

        Assert.Equal(totalQueueCount, names.Count);
        Assert.Equal(totalQueueCount, names.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void Prefixes_Every_Name_With_The_Service_Id_And_Streaming_Segment()
    {
        var names = KurrentDBQueueStreamProviderUtils.GenerateDefaultQueueNames("service", "provider", 4);

        Assert.All(names, name => Assert.StartsWith("service/streaming/", name, StringComparison.Ordinal));
    }

    [Fact]
    public void Uses_The_Provider_Name_In_The_Queue_Id()
    {
        var name = Assert.Single(KurrentDBQueueStreamProviderUtils.GenerateDefaultQueueNames("service", "provider", 1));

        Assert.Contains("provider", name[("service/streaming/".Length)..], StringComparison.Ordinal);
    }

    [Fact]
    public void Generates_The_Same_Names_Every_Time()
    {
        Assert.Equal(
            KurrentDBQueueStreamProviderUtils.GenerateDefaultQueueNames("service", "provider", 8),
            KurrentDBQueueStreamProviderUtils.GenerateDefaultQueueNames("service", "provider", 8));
    }

    [Fact]
    public void Different_Providers_Get_Different_Names()
    {
        var first = KurrentDBQueueStreamProviderUtils.GenerateDefaultQueueNames("service", "first", 4);
        var second = KurrentDBQueueStreamProviderUtils.GenerateDefaultQueueNames("service", "second", 4);

        Assert.Empty(first.Intersect(second, StringComparer.Ordinal));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Rejects_A_Queue_Count_Below_One(int totalQueueCount)
    {
        Assert.ThrowsAny<ArgumentException>(() => KurrentDBQueueStreamProviderUtils.GenerateDefaultQueueNames("service", "provider", totalQueueCount));
    }
}
