using System.Text;

namespace Continuum.Streaming.Orleans.KurrentDB.Abstractions.Tests;

/// <summary>
///     Verifies how <see cref="KurrentDBEventMetadataCodec" /> reads the JSON document KurrentDB stores in an event's
///     metadata slot.
/// </summary>
/// <remarks>
///     The slot is shared: the client merges its own "$traceId" and "$spanId" into whatever the producer supplied.
///     These tests pin the two behaviours the read path depends on. Reserved tracing entries must be lifted onto their
///     own values rather than leaking into the metadata bag, and a document the codec cannot make sense of must yield
///     an empty result instead of throwing, because a silo reading $all sees every event on the log, including events
///     written by applications that use the slot for something else entirely.
/// </remarks>
public class KurrentDBEventMetadataCodecTests
{
    /// <summary>
    ///     An event with no metadata at all, which is what a producer that supplies none writes when no Activity is recording.
    /// </summary>
    [Fact]
    public void Read_Returns_An_Empty_Result_For_Absent_Metadata()
    {
        // Act
        var result = KurrentDBEventMetadataCodec.Read([]);

        // Assert
        Assert.Null(result.TraceId);
        Assert.Null(result.SpanId);
        Assert.Empty(result);
    }

    /// <summary>
    ///     The event sourcing shape: the producer supplied nothing and the client injected tracing on its own.
    /// </summary>
    [Fact]
    public void Read_Lifts_Tracing_And_Leaves_The_Metadata_Bag_Empty()
    {
        // Arrange
        var metadata = Utf8("""{"$traceId":"0af7651916cd43dd8448eb211c80319c","$spanId":"b7ad6b7169203331"}""");

        // Act
        var result = KurrentDBEventMetadataCodec.Read(metadata);

        // Assert
        Assert.Equal("0af7651916cd43dd8448eb211c80319c", result.TraceId);
        Assert.Equal("b7ad6b7169203331", result.SpanId);
        Assert.Empty(result);
    }

    /// <summary>
    ///     The merged shape: the producer supplied metadata and the client added tracing alongside it.
    /// </summary>
    [Fact]
    public void Read_Separates_Producer_Metadata_From_Injected_Tracing()
    {
        // Arrange
        var metadata = Utf8(
            """{"correlationId":"abc-123","$traceId":"0af7651916cd43dd8448eb211c80319c","tenant":"contoso","$spanId":"b7ad6b7169203331"}""");

        // Act
        var result = KurrentDBEventMetadataCodec.Read(metadata);

        // Assert
        Assert.Equal("0af7651916cd43dd8448eb211c80319c", result.TraceId);
        Assert.Equal("b7ad6b7169203331", result.SpanId);
        Assert.Equal("abc-123", result.GetValueOrDefault("correlationId"));
        Assert.Equal("contoso", result.GetValueOrDefault("tenant"));
        Assert.Equal(2, result.Count);
    }

    /// <summary>
    ///     Reserved names belong to the transport, so any beyond the two lifted ones must not reach consumers as metadata.
    /// </summary>
    [Fact]
    public void Read_Drops_Reserved_Names_It_Does_Not_Understand()
    {
        // Arrange
        var metadata = Utf8("""{"$somethingNew":"reserved","kept":"value"}""");

        // Act
        var result = KurrentDBEventMetadataCodec.Read(metadata);

        // Assert
        Assert.Equal("value", result.GetValueOrDefault("kept"));
        Assert.Single(result);
    }

    /// <summary>
    ///     Values are surfaced as text. Non-string JSON keeps its raw form so nothing is silently dropped, but the codec
    ///     never interprets it, because nothing at this layer knows what type a nested value was meant to be.
    /// </summary>
    [Fact]
    public void Read_Surfaces_Non_String_Values_As_Raw_Json_Text()
    {
        // Arrange
        var metadata = Utf8("""{"attempt":3,"replayed":true,"origin":{"silo":"one"},"ignored":null}""");

        // Act
        var result = KurrentDBEventMetadataCodec.Read(metadata);

        // Assert
        Assert.Equal("3", result.GetValueOrDefault("attempt"));
        Assert.Equal("true", result.GetValueOrDefault("replayed"));
        Assert.Equal("""{"silo":"one"}""", result.GetValueOrDefault("origin"));
        Assert.False(result.ContainsKey("ignored"));
    }

    /// <summary>
    ///     A foreign writer may put anything in the slot. One unreadable document must not stall the queue.
    /// </summary>
    [Theory]
    [InlineData("not json at all")]
    [InlineData("{\"unterminated\":")]
    [InlineData("[1,2,3]")]
    [InlineData("\"a bare string\"")]
    public void Read_Returns_An_Empty_Result_For_Metadata_It_Cannot_Use(string content)
    {
        // Act
        var result = KurrentDBEventMetadataCodec.Read(Utf8(content));

        // Assert
        Assert.Null(result.TraceId);
        Assert.Null(result.SpanId);
        Assert.Empty(result);
    }

    /// <summary>
    ///     Nothing to write means no document, so the codec does not claim a slot it has no use for.
    /// </summary>
    [Fact]
    public void Write_Returns_Null_When_There_Is_No_Metadata()
    {
        Assert.Null(KurrentDBEventMetadataCodec.Write(null));
        Assert.Null(KurrentDBEventMetadataCodec.Write(StreamedEventMetadata.Empty));
    }

    /// <summary>
    ///     What the codec writes must survive a round trip, and tracing must not be written by the producer, because the
    ///     client injects the activity actually in effect at append time.
    /// </summary>
    [Fact]
    public void Write_Round_Trips_Producer_Metadata_Without_Tracing()
    {
        // Arrange
        var metadata = StreamedEventMetadata.Create(
        [
            new KeyValuePair<string, string?>("correlationId", "abc-123"),
            new KeyValuePair<string, string?>("tenant", "contoso"),
        ]);

        // Act
        var written = KurrentDBEventMetadataCodec.Write(metadata);

        // Assert
        Assert.NotNull(written);
        var result = KurrentDBEventMetadataCodec.Read(written);
        Assert.Null(result.TraceId);
        Assert.Null(result.SpanId);
        Assert.Equal("abc-123", result.GetValueOrDefault("correlationId"));
        Assert.Equal("contoso", result.GetValueOrDefault("tenant"));
        Assert.Equal(2, result.Count);
    }

    private static byte[] Utf8(string value) => Encoding.UTF8.GetBytes(value);
}
