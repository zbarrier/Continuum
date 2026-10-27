using Orleans.Configuration;
using Orleans.Runtime;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

public class DefaultStreamIdMapperTests
{
    [Fact]
    public void Maps_A_Stream_Without_A_Tenant_To_Category_And_Key()
    {
        Assert.Equal(StreamId.Create("snack", "123"), KurrentDBDataAdapterOptions.DefaultStreamIdMapper("snack-123"));
    }

    [Fact]
    public void Keeps_The_Tenant_In_The_Key()
    {
        Assert.Equal(StreamId.Create("snack", "acme|123"), KurrentDBDataAdapterOptions.DefaultStreamIdMapper("snack-acme|123"));
    }

    [Fact]
    public void Keeps_Separators_In_A_Tenant_Qualified_Guid_Key()
    {
        var key = "acme|" + Guid.NewGuid();

        Assert.Equal(StreamId.Create("snack", key), KurrentDBDataAdapterOptions.DefaultStreamIdMapper("snack-" + key));
    }

    [Theory]
    [InlineData("snack")]
    [InlineData("-123")]
    [InlineData("snack-")]
    public void Maps_A_Name_Without_A_Usable_Separator_To_An_Empty_Namespace(string streamName)
    {
        Assert.Equal(StreamId.Create(string.Empty, streamName), KurrentDBDataAdapterOptions.DefaultStreamIdMapper(streamName));
    }
}
