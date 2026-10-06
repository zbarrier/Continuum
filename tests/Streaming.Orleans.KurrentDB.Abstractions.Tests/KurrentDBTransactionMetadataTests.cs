using System.Text;

namespace Continuum.Streaming.Orleans.KurrentDB.Abstractions.Tests;

/// <summary>
///     Verifies how <see cref="KurrentDBEventMetadataCodec" /> carries the transaction an event was appended in.
/// </summary>
/// <remarks>
///     KurrentDB reports the same commit and prepare position for every event of an append, so the grouping cannot be
///     recovered from the log and has to be recorded by the producer. These tests pin the behaviours the assembler
///     depends on. A group must round trip exactly, and a group that is absent or does not describe a consistent shape
///     must be reported as no transaction at all, because the caller then delivers the event immediately as its own
///     transaction. Trusting a bad count instead would leave a batch waiting for events that never arrive.
/// </remarks>
public class KurrentDBTransactionMetadataTests
{
    /// <summary>
    ///     The ordinary case: a multi event append writes a group that reads back unchanged.
    /// </summary>
    [Fact]
    public void Write_Round_Trips_A_Transaction()
    {
        // Arrange
        var transactionId = NewId.Next();
        var transaction = new KurrentDBEventMetadataCodec.TransactionInfo(transactionId, 3, 3, 1);

        // Act
        var written = KurrentDBEventMetadataCodec.Write(null, transaction);
        var result = KurrentDBEventMetadataCodec.Read(written);

        // Assert
        Assert.NotNull(written);
        Assert.Equal(transactionId, result.TransactionId);
        Assert.Equal(3, result.TransactionSize);
        Assert.Equal(3, result.TransactionPartitionSize);
        Assert.Equal(1, result.TransactionPartitionIndex);
    }

    /// <summary>
    ///     A transaction must be written even when the producer supplied no metadata of its own, otherwise an event
    ///     sourced append would lose its grouping.
    /// </summary>
    [Fact]
    public void Write_Emits_A_Document_For_A_Transaction_Without_Producer_Metadata()
    {
        // Arrange
        var transaction = new KurrentDBEventMetadataCodec.TransactionInfo(NewId.Next(), 1, 1, 0);

        // Act
        var written = KurrentDBEventMetadataCodec.Write(null, transaction);

        // Assert
        Assert.NotNull(written);
        Assert.NotEmpty(written);
    }

    /// <summary>
    ///     Nothing to write at all still yields nothing, so an event with neither metadata nor a transaction does not
    ///     gain an empty document.
    /// </summary>
    [Fact]
    public void Write_Returns_Null_When_There_Is_No_Metadata_And_No_Transaction()
    {
        // Act
        var written = KurrentDBEventMetadataCodec.Write(null, null);

        // Assert
        Assert.Null(written);
    }

    /// <summary>
    ///     Producer metadata and the transaction share one document and must not interfere with each other.
    /// </summary>
    [Fact]
    public void Write_Keeps_Producer_Metadata_Alongside_The_Transaction()
    {
        // Arrange
        var metadata = StreamedEventMetadata.Create([new KeyValuePair<string, string?>("tenant", "acme")]);
        var transaction = new KurrentDBEventMetadataCodec.TransactionInfo(NewId.Next(), 2, 2, 0);

        // Act
        var result = KurrentDBEventMetadataCodec.Read(KurrentDBEventMetadataCodec.Write(metadata, transaction));

        // Assert
        Assert.Equal("acme", result.GetValueOrDefault("tenant"));
        Assert.Equal(transaction.TransactionId, result.TransactionId);
        Assert.Equal(transaction.TransactionSize, result.TransactionSize);
        Assert.Equal(transaction.TransactionPartitionSize, result.TransactionPartitionSize);
        Assert.Equal(transaction.TransactionPartitionIndex, result.TransactionPartitionIndex);
    }

    /// <summary>
    ///     The transaction entries are lifted onto <c>Transaction</c>, so consumers have one representation of the
    ///     grouping rather than also finding it in the metadata bag.
    /// </summary>
    [Fact]
    public void Read_Lifts_The_Transaction_Out_Of_The_Metadata_Bag()
    {
        // Arrange
        var transaction = new KurrentDBEventMetadataCodec.TransactionInfo(NewId.Next(), 2, 2, 1);

        // Act
        var result = KurrentDBEventMetadataCodec.Read(KurrentDBEventMetadataCodec.Write(null, transaction));

        // Assert
        Assert.Empty(result);
        Assert.NotNull(result.TransactionId);
    }

    /// <summary>
    ///     An event written by a foreign producer, which carries metadata but knows nothing of transactions.
    /// </summary>
    [Fact]
    public void Read_Reports_No_Transaction_For_A_Foreign_Event()
    {
        // Arrange
        var metadata = Encoding.UTF8.GetBytes("""{"source":"legacy"}""");

        // Act
        var result = KurrentDBEventMetadataCodec.Read(metadata);

        // Assert
        Assert.Null(result.TransactionId);
        Assert.Equal("legacy", result.GetValueOrDefault("source"));
    }

    /// <summary>
    ///     A group missing one of its entries cannot be assembled, so it is discarded rather than half trusted.
    /// </summary>
    [Fact]
    public void Read_Reports_No_Transaction_When_The_Group_Is_Incomplete()
    {
        // Arrange
        var metadata = Encoding.UTF8.GetBytes($$"""{"transactionId":"{{Guid.NewGuid()}}","transactionSize":"3"}""");

        // Act
        var result = KurrentDBEventMetadataCodec.Read(metadata);

        // Assert
        Assert.Null(result.TransactionId);
    }

    /// <summary>
    ///     A group whose counts contradict each other would strand the assembler, so it is discarded.
    /// </summary>
    /// <remarks>
    ///     The per stream count can never exceed the total, and the ordinal must fall inside the per stream count.
    /// </remarks>
    [Theory]
    [InlineData("3", "5", "0")] // partition size larger than the transaction
    [InlineData("3", "3", "3")] // ordinal past the end of the partition
    [InlineData("0", "0", "0")] // an empty transaction
    [InlineData("3", "3", "-1")] // negative ordinal
    [InlineData("nonsense", "3", "0")] // unparsable count
    public void Read_Reports_No_Transaction_For_An_Inconsistent_Group(string transactionSize, string partitionSize, string partitionIndex)
    {
        // Arrange
        var metadata = Encoding.UTF8.GetBytes($$"""
            {"transactionId":"{{Guid.NewGuid()}}","transactionSize":"{{transactionSize}}","transactionPartitionSize":"{{partitionSize}}","transactionPartitionIndex":"{{partitionIndex}}"}
            """);

        // Act
        var result = KurrentDBEventMetadataCodec.Read(metadata);

        // Assert
        Assert.Null(result.TransactionId);
    }

    /// <summary>
    ///     A transaction that also wrote to other streams is still readable here; the caller uses the two counts to
    ///     decide it has everything it will ever receive for its own stream.
    /// </summary>
    [Fact]
    public void Read_Preserves_Both_Counts_For_A_Cross_Stream_Transaction()
    {
        // Arrange
        var transaction = new KurrentDBEventMetadataCodec.TransactionInfo(NewId.Next(), 5, 2, 1);

        // Act
        var result = KurrentDBEventMetadataCodec.Read(KurrentDBEventMetadataCodec.Write(null, transaction));

        // Assert
        Assert.NotNull(result.TransactionId);
        Assert.Equal(5, result.TransactionSize);
        Assert.Equal(2, result.TransactionPartitionSize);
    }
}
