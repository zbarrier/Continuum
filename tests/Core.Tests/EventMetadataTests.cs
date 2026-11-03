using Continuum.Events;

namespace Continuum.Tests;

public sealed class EventMetadataTests
{
    [Theory]
    [InlineData("$correlation", true)]
    [InlineData("$", true)]
    [InlineData("correlation", false)]
    [InlineData("a$b", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsReservedName_matches_only_the_reserved_prefix(string? name, bool expected)
    {
        Assert.Equal(expected, IEventMetadata.IsReservedName(name!));
    }
}
