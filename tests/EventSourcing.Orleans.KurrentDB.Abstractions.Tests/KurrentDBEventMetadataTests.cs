namespace Continuum.EventSourcing.Orleans.KurrentDB.Abstractions.Tests;

public class KurrentDBEventMetadataTests
{
    [Fact]
    public void Create_Returns_Empty_When_Nothing_Is_Supplied()
    {
        Assert.Same(KurrentDBEventMetadata.Empty, KurrentDBEventMetadata.Create(null));
        Assert.Same(KurrentDBEventMetadata.Empty, KurrentDBEventMetadata.Create([new("$traceId", "t"), new("", "x"), new("name", null)]));
    }

    [Fact]
    public void Create_Skips_Reserved_Empty_And_Null_Entries()
    {
        var metadata = KurrentDBEventMetadata.Create([new("$reserved", "x"), new("", "x"), new("nulled", null), new("name", "value")]);

        var entry = Assert.Single(metadata);
        Assert.Equal("name", entry.Key);
        Assert.Equal("value", entry.Value);
    }

    [Fact]
    public void Create_Compares_Names_Ordinally()
    {
        var metadata = KurrentDBEventMetadata.Create([new("Name", "upper"), new("name", "lower")]);

        Assert.Equal(2, metadata.Count);
        Assert.Equal("upper", metadata["Name"]);
        Assert.Equal("lower", metadata["name"]);
        Assert.False(metadata.ContainsKey("NAME"));
    }

    [Fact]
    public void Create_Keeps_Tracing_Without_Entries()
    {
        var metadata = KurrentDBEventMetadata.Create(null, "trace", "span", null);

        Assert.NotSame(KurrentDBEventMetadata.Empty, metadata);
        Assert.Equal("trace", metadata.TraceId);
        Assert.Equal("span", metadata.SpanId);
        Assert.Empty(metadata);
    }

    [Fact]
    public void Create_Keeps_A_Valid_Transaction_Without_Entries()
    {
        var transaction = new TransactionInfo(NewId.Next(), 3, 2, 1);
        var metadata = KurrentDBEventMetadata.Create(null, null, null, transaction);

        Assert.Equal(transaction.TransactionId, metadata.TransactionId);
        Assert.Equal(3, metadata.TransactionSize);
        Assert.Equal(2, metadata.TransactionPartitionSize);
        Assert.Equal(1, metadata.TransactionPartitionIndex);
        Assert.Empty(metadata);
    }

    [Fact]
    public void Create_Rejects_An_Inconsistent_Transaction()
    {
        var exception = Assert.Throws<ArgumentException>(() => KurrentDBEventMetadata.Create(null, null, null, new TransactionInfo(NewId.Next(), 1, 1, 1)));
        Assert.Equal("transaction", exception.ParamName);
    }

    [Theory]
    [InlineData("$traceId", true)]
    [InlineData("$", true)]
    [InlineData("trace$", false)]
    [InlineData("", false)]
    public void IsReservedName_Checks_The_Prefix(string name, bool expected)
    {
        Assert.Equal(expected, KurrentDBEventMetadata.IsReservedName(name));
    }
}
