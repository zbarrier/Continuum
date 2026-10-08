using Continuum.Serialization.Orleans;
using Continuum.TypeMapping;

namespace Continuum.Streaming.Orleans.Tests;

/// <summary>
///     Verifies that streamed events held in grain state survive the System.Text.Json state serializer, with the
///     payload type restored through the type mapper.
/// </summary>
public class StreamedEventJsonStorageTests
{
    private static readonly TypeMappedJsonGrainStorageSerializer Serializer = CreateSerializer();

    [Fact]
    public void Round_Trips_Event_Payload_And_Metadata()
    {
        var transactionId = NewId.Next();
        var metadata = StreamedEventMetadata.Create(
            [new("tenant", "acme")], "trace-1", "span-1", transactionId, 3, 2, 1);
        var original = Event(metadata);

        var restored = RoundTrip(new Holder { Event = original }).Event!;

        Assert.Equal(original.EventId, restored.EventId);
        Assert.Equal(original.SequenceNumber, restored.SequenceNumber);
        Assert.Equal(original.Timestamp, restored.Timestamp);
        Assert.Equal("payload", Assert.IsType<Payload>(restored.Event).Text);
        Assert.Equal("acme", restored.Metadata.GetValueOrDefault("tenant"));
        Assert.Equal("trace-1", restored.Metadata.TraceId);
        Assert.Equal("span-1", restored.Metadata.SpanId);
        Assert.Equal(transactionId, restored.Metadata.TransactionId);
        Assert.Equal(3, restored.Metadata.TransactionSize);
        Assert.Equal(2, restored.Metadata.TransactionPartitionSize);
        Assert.Equal(1, restored.Metadata.TransactionPartitionIndex);
    }

    [Fact]
    public void Round_Trips_Empty_Metadata_And_Null_Event()
    {
        Assert.Empty(RoundTrip(new Holder { Event = Event(null) }).Event!.Metadata);
        Assert.Null(RoundTrip(new Holder()).Event);
    }

    private static Holder RoundTrip(Holder holder) => Serializer.Deserialize<Holder>(Serializer.Serialize(holder));

    private static StreamedEvent<object> Event(StreamedEventMetadata? metadata) =>
        new(NewId.Next(), "Tests.Payload", "counted-1", "1", "counted", "0", 0, 0, 1, 0, DateTime.UtcNow, new Payload("payload"), metadata);

    private static TypeMappedJsonGrainStorageSerializer CreateSerializer()
    {
        var mapper = new DefaultTypeMapAttributeMapper();
        mapper.AddType(typeof(Payload), "Tests.Payload");
        mapper.Freeze();

        var options = new System.Text.Json.JsonSerializerOptions(TypeMappedJsonGrainStorageSerializer.DefaultOptions);
        options.Converters.Add(new StreamedEventJsonConverter(mapper));
        return new TypeMappedJsonGrainStorageSerializer(options);
    }

    public sealed class Holder
    {
        public IStreamedEvent<object>? Event { get; set; }
    }

    public sealed record Payload(string Text);
}
