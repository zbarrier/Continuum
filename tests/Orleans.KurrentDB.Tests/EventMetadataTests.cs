namespace Continuum.Orleans.KurrentDB.Tests;

public class EventMetadataTests
{
    [Fact]
    public void Create_Returns_Empty_When_Nothing_Is_Supplied()
    {
        Assert.Same(EventMetadata.Empty, EventMetadata.Create(null));
        Assert.Same(EventMetadata.Empty, EventMetadata.Create([new("$traceId", "t"), new("", "x"), new("name", null)]));
    }

    [Fact]
    public void Create_Skips_Reserved_Empty_And_Null_Entries()
    {
        var metadata = EventMetadata.Create([new("$reserved", "x"), new("", "x"), new("nulled", null), new("name", "value")]);

        var entry = Assert.Single(metadata);
        Assert.Equal("name", entry.Key);
        Assert.Equal("value", entry.Value);
    }

    [Fact]
    public void Create_Compares_Names_Ordinally()
    {
        var metadata = EventMetadata.Create([new("Name", "upper"), new("name", "lower")]);

        Assert.Equal(2, metadata.Count);
        Assert.Equal("upper", metadata["Name"]);
        Assert.Equal("lower", metadata["name"]);
        Assert.False(metadata.ContainsKey("NAME"));
    }

    [Fact]
    public void Create_Keeps_Tracing_Without_Entries()
    {
        var metadata = EventMetadata.Create(null, "trace", "span", null);

        Assert.NotSame(EventMetadata.Empty, metadata);
        Assert.Equal("trace", metadata.TraceId);
        Assert.Equal("span", metadata.SpanId);
        Assert.Empty(metadata);
    }

    [Fact]
    public void Create_Keeps_A_Valid_Transaction_Without_Entries()
    {
        var transaction = new TransactionInfo(Guid.CreateVersion7(), 3, 2, 1);
        var metadata = EventMetadata.Create(null, null, null, transaction);

        Assert.Equal(transaction.TransactionId, metadata.TransactionId);
        Assert.Equal(3, metadata.TransactionSize);
        Assert.Equal(2, metadata.TransactionPartitionSize);
        Assert.Equal(1, metadata.TransactionPartitionIndex);
        Assert.Empty(metadata);
    }

    [Fact]
    public void Create_Rejects_An_Inconsistent_Transaction()
    {
        var exception = Assert.Throws<ArgumentException>(() => EventMetadata.Create(null, null, null, new TransactionInfo(Guid.CreateVersion7(), 1, 1, 1)));
        Assert.Equal("transaction", exception.ParamName);
    }

    [Theory]
    [InlineData("$traceId", true)]
    [InlineData("$", true)]
    [InlineData("trace$", false)]
    [InlineData("", false)]
    public void IsReservedName_Checks_The_Prefix(string name, bool expected)
    {
        Assert.Equal(expected, IEventMetadata.IsReservedName(name));
    }
}
