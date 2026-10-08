using Microsoft.Extensions.Options;

using Orleans.Serialization;
using Orleans.Storage;

namespace Continuum.Streaming.Orleans.Tests;

/// <summary>
///     Verifies that streamed events survive Orleans' default Newtonsoft grain storage serializer, which grains
///     use when they keep a streamed event in their persistent state.
/// </summary>
public class StreamedEventNewtonsoftStorageTests
{
    private static readonly JsonGrainStorageSerializer Serializer =
        new(new OrleansJsonSerializer(Options.Create(new OrleansJsonSerializerOptions())));

    [Fact]
    public void Round_Trips_Metadata_Entries_And_Tracing()
    {
        var transactionId = NewId.Next();
        var metadata = StreamedEventMetadata.Create(
            [new("tenant", "acme")], "trace-1", "span-1", transactionId, 3, 2, 1);

        var restored = RoundTrip(new Holder { Event = Event(metadata) }).Event!.Metadata;

        Assert.Equal("acme", restored.GetValueOrDefault("tenant"));
        Assert.Equal("trace-1", restored.TraceId);
        Assert.Equal("span-1", restored.SpanId);
        Assert.Equal(transactionId, restored.TransactionId);
        Assert.Equal(3, restored.TransactionSize);
        Assert.Equal(2, restored.TransactionPartitionSize);
        Assert.Equal(1, restored.TransactionPartitionIndex);
    }

    [Fact]
    public void Round_Trips_Empty_Metadata()
    {
        var restored = RoundTrip(new Holder { Event = Event(null) }).Event!;

        Assert.Empty(restored.Metadata);
        Assert.Equal("payload", ((Payload)restored.Event).Text);
    }

    private static Holder RoundTrip(Holder holder) => Serializer.Deserialize<Holder>(Serializer.Serialize(holder))!;

    private static StreamedEvent<object> Event(StreamedEventMetadata? metadata) =>
        new(NewId.Next(), "payload", "counted-1", "1", "counted", "0", 0, 0, 1, 0, DateTime.UtcNow, new Payload("payload"), metadata);

    public sealed class Holder
    {
        public IStreamedEvent<object>? Event { get; set; }
    }

    public sealed record Payload(string Text);
}
