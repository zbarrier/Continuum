using System.Text;
using System.Text.Json;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Abstractions.Tests;

public class KurrentDBEventMetadataCodecTests
{
    private static byte[] Utf8(string json) => Encoding.UTF8.GetBytes(json);

    private static Dictionary<string, JsonElement> Parse(byte[]? bytes) => JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(bytes!)!;

    private static string Transaction(Guid id, string size, string partitionSize, string partitionIndex) =>
        $"{{\"transactionId\":\"{id}\",\"transactionSize\":\"{size}\",\"transactionPartitionSize\":\"{partitionSize}\",\"transactionPartitionIndex\":\"{partitionIndex}\"}}";

    [Theory]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("[1,2,3]")]
    [InlineData("\"text\"")]
    [InlineData("42")]
    [InlineData("{\"unterminated\":")]
    public void Read_Returns_Empty_For_Unusable_Documents(string json)
    {
        Assert.Same(KurrentDBEventMetadata.Empty, KurrentDBEventMetadataCodec.Read(Utf8(json)));
    }

    [Fact]
    public void Read_Lifts_Tracing_And_Drops_Unknown_Reserved_Names()
    {
        var metadata = KurrentDBEventMetadataCodec.Read(Utf8("{\"$traceId\":\"trace\",\"$spanId\":\"span\",\"$future\":\"x\",\"name\":\"value\"}"));

        Assert.Equal("trace", metadata.TraceId);
        Assert.Equal("span", metadata.SpanId);
        var entry = Assert.Single(metadata);
        Assert.Equal("name", entry.Key);
        Assert.Equal("value", entry.Value);
    }

    [Fact]
    public void Read_Keeps_Raw_Json_For_Non_String_Values_And_Skips_Nulls()
    {
        var metadata = KurrentDBEventMetadataCodec.Read(Utf8("{\"number\":12.50,\"flag\":true,\"object\":{\"a\":1},\"missing\":null}"));

        Assert.Equal(3, metadata.Count);
        Assert.Equal("12.50", metadata["number"]);
        Assert.Equal("true", metadata["flag"]);
        Assert.Equal("{\"a\":1}", metadata["object"]);
    }

    [Fact]
    public void Read_Lifts_A_Complete_Transaction()
    {
        var id = NewId.Next();
        var metadata = KurrentDBEventMetadataCodec.Read(Utf8(Transaction(id.ToSequentialGuid(), "3", "2", "1")));

        Assert.Equal(id, metadata.TransactionId);
        Assert.Equal(3, metadata.TransactionSize);
        Assert.Equal(2, metadata.TransactionPartitionSize);
        Assert.Equal(1, metadata.TransactionPartitionIndex);
        Assert.Empty(metadata);
    }

    [Theory]
    [InlineData("three", "2", "1")]
    [InlineData("3", "2", "-1")]
    [InlineData("3", "2", " 1")]
    [InlineData("1", "2", "0")]
    [InlineData("3", "2", "2")]
    public void Read_Discards_An_Inconsistent_Transaction(string size, string partitionSize, string partitionIndex)
    {
        var metadata = KurrentDBEventMetadataCodec.Read(Utf8(Transaction(Guid.NewGuid(), size, partitionSize, partitionIndex)));

        Assert.Null(metadata.TransactionId);
        Assert.Null(metadata.TransactionSize);
        Assert.Empty(metadata);
    }

    [Fact]
    public void Read_Discards_A_Partial_Transaction()
    {
        var metadata = KurrentDBEventMetadataCodec.Read(Utf8($"{{\"transactionId\":\"{Guid.NewGuid()}\",\"transactionSize\":\"3\"}}"));

        Assert.Null(metadata.TransactionId);
        Assert.Empty(metadata);
    }

    [Fact]
    public void Write_Returns_Null_When_There_Is_Nothing_To_Write()
    {
        Assert.Null(KurrentDBEventMetadataCodec.Write(null));
        Assert.Null(KurrentDBEventMetadataCodec.Write(KurrentDBEventMetadata.Empty));
        Assert.Null(KurrentDBEventMetadataCodec.WriteAll(null));
        Assert.Null(KurrentDBEventMetadataCodec.WriteAll(KurrentDBEventMetadata.Empty));
    }

    [Fact]
    public void Write_Rejects_An_Inconsistent_Transaction()
    {
        var exception = Assert.Throws<ArgumentException>(() => KurrentDBEventMetadataCodec.Write(null, new TransactionInfo(NewId.Next(), 1, 2, 0)));
        Assert.Equal("transaction", exception.ParamName);
    }

    [Fact]
    public void Write_Omits_Tracing()
    {
        var metadata = KurrentDBEventMetadata.Create([new("name", "value")], "trace", "span", null);

        var entry = Assert.Single(Parse(KurrentDBEventMetadataCodec.Write(metadata)));
        Assert.Equal("name", entry.Key);
        Assert.Equal("value", entry.Value.GetString());
    }

    [Fact]
    public void Write_Records_The_Transaction_As_Invariant_Strings()
    {
        var transaction = new TransactionInfo(NewId.Next(), 12, 10, 9);

        var document = Parse(KurrentDBEventMetadataCodec.Write(null, transaction));

        Assert.Equal(transaction.TransactionId.ToSequentialGuid().ToString("D"), document[KurrentDBEventMetadataCodec.TransactionIdName].GetString());
        Assert.Equal("12", document[KurrentDBEventMetadataCodec.TransactionSizeName].GetString());
        Assert.Equal("10", document[KurrentDBEventMetadataCodec.TransactionPartitionSizeName].GetString());
        Assert.Equal("9", document[KurrentDBEventMetadataCodec.TransactionPartitionIndexName].GetString());
    }

    [Fact]
    public void Write_Then_Read_Round_Trips_Entries_And_Transaction()
    {
        var transaction = new TransactionInfo(NewId.Next(), 3, 2, 1);
        var metadata = KurrentDBEventMetadata.Create([new("a", "1"), new("b", "two")]);

        var read = KurrentDBEventMetadataCodec.Read(KurrentDBEventMetadataCodec.Write(metadata, transaction));

        Assert.Equal(metadata.OrderBy(kv => kv.Key), read.OrderBy(kv => kv.Key));
        Assert.Equal(transaction.TransactionId, read.TransactionId);
        Assert.Equal(transaction.TransactionPartitionIndex, read.TransactionPartitionIndex);
        Assert.Null(read.TraceId);
    }

    [Fact]
    public void Write_Handles_Documents_Larger_Than_The_Initial_Buffer()
    {
        var entries = Enumerable.Range(0, 50).Select(i => new KeyValuePair<string, string?>($"key{i}", new string('x', 40)));

        var read = KurrentDBEventMetadataCodec.Read(KurrentDBEventMetadataCodec.Write(KurrentDBEventMetadata.Create(entries)));

        Assert.Equal(50, read.Count);
    }

    [Fact]
    public void WriteAll_Round_Trips_Tracing_And_Transaction()
    {
        var transaction = new TransactionInfo(NewId.Next(), 3, 2, 1);
        var metadata = KurrentDBEventMetadata.Create([new("name", "value")], "trace", "span", transaction);

        var read = KurrentDBEventMetadataCodec.Read(KurrentDBEventMetadataCodec.WriteAll(metadata));

        Assert.Equal("trace", read.TraceId);
        Assert.Equal("span", read.SpanId);
        Assert.Equal("value", read["name"]);
        Assert.Equal(transaction.TransactionId, read.TransactionId);
        Assert.Equal(transaction.TransactionPartitionIndex, read.TransactionPartitionIndex);
    }
}
