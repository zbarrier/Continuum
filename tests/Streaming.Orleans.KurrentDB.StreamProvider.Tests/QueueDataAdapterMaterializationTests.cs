using System.Text;

using Continuum.Orleans.Streams;
using Continuum.Streaming;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;

using Orleans.Configuration;
using Continuum.Streaming.Orleans.KurrentDB.StreamProvider;
using Orleans.Runtime;
using Orleans.Serialization;
using Orleans.Storage;
using Orleans.Streams;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies that the queue data adapters never hand a subscriber an event that has no payload, cannot be read, or
///     reads as null, and that unknown event types still follow the configured behavior.
/// </summary>
public class QueueDataAdapterMaterializationTests
{
    private const string KnownType = "test-known-event";

    private sealed record KnownEvent(string Value);

    private sealed class FakeTypeMapper : TypeMapper;

    private sealed class FakeSerializer(Func<BinaryData, object?> deserialize) : IGrainStorageSerializer
    {
        public BinaryData Serialize<T>(T? input) => throw new NotSupportedException();

        public T Deserialize<T>(BinaryData input) => (T)deserialize(input)!;
    }

    private static readonly Serializer OrleansSerializer = new ServiceCollection().AddSerializer().BuildServiceProvider().GetRequiredService<Serializer>();

    private static IEnumerable<Tuple<IStreamedEvent<object>, StreamSequenceToken>> Materialize(string eventType, byte[] data, Func<BinaryData, object?> deserialize, KurrentDBUnknownEventTypeBehavior unknownBehavior = KurrentDBUnknownEventTypeBehavior.Halt)
    {
        var typeMapper = new FakeTypeMapper();
        typeMapper.AddType(typeof(KnownEvent), KnownType);
        var options = new KurrentDBDataAdapterOptions
        {
            ConnectionName = "test",
            ProviderName = "test",
            TypeMapper = typeMapper,
            GrainStorageSerializer = new FakeSerializer(deserialize),
            UnknownEventTypeBehavior = unknownBehavior,
        };
        var message = new KurrentDBMessage("test", StreamId.Create("test", "1"), 0, 1, Guid.NewGuid(), eventType, data, new StreamName("test-1", "test", null, "1"));
        IBatchContainer container = new TestAdapter(options).Build(message);
        return container.GetEvents<IStreamedEvent<object>>().ToList();
    }

    private sealed class TestAdapter(KurrentDBDataAdapterOptions options) : KurrentDBQueueDataAdapter(OrleansSerializer, options)
    {
        public IBatchContainer Build(KurrentDBMessage message) => GetBatchContainer(message);
    }

    private static readonly byte[] Payload = Encoding.UTF8.GetBytes("{}");

    [Fact]
    public void Delivers_resolved_event()
    {
        var events = Materialize(KnownType, Payload, _ => new KnownEvent("a"));

        Assert.Equal(new KnownEvent("a"), Assert.Single(events).Item1.Event);
    }

    [Fact]
    public void Skips_empty_payload()
    {
        var events = Materialize(KnownType, [], _ => throw new InvalidOperationException("Should not be called."));

        Assert.Empty(events);
    }

    [Fact]
    public void Skips_payload_that_cannot_be_deserialized()
    {
        var events = Materialize(KnownType, Payload, _ => throw new FormatException("Corrupt."));

        Assert.Empty(events);
    }

    [Fact]
    public void Skips_payload_that_deserializes_to_null()
    {
        var events = Materialize(KnownType, Payload, _ => null);

        Assert.Empty(events);
    }

    [Fact]
    public void Skips_unknown_type_when_configured_to_skip()
    {
        var events = Materialize("unknown-type", Payload, _ => new KnownEvent("a"), KurrentDBUnknownEventTypeBehavior.Skip);

        Assert.Empty(events);
    }

    [Fact]
    public void Faults_unknown_type_when_configured_to_halt()
    {
        Assert.Throws<InvalidOperationException>(() => Materialize("unknown-type", Payload, _ => new KnownEvent("a")));
    }
}
