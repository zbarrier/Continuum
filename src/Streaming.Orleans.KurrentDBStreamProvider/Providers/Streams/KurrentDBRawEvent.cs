namespace Orleans.Providers.Streams.KurrentDB;

/// <summary>
///     The undeserialized payload of a KurrentDB event as it was read from the log.
/// </summary>
/// <param name="Data">
///     The raw event body. This provider writes an Orleans serialized batch here, so it stays opaque until a consumer
///     reads it and the batch container materializes it.
/// </param>
/// <param name="Metadata">
///     The raw event metadata, which holds the Orleans <see cref="Runtime.StreamId" /> the event belongs to.
/// </param>
/// <remarks>
///     The payload is kept as bytes rather than a deserialized object so it can be copied straight into the cache
///     buffers. Deserializing in the receiver would materialize every event, including the ones no consumer ever reads.
/// </remarks>
[GenerateSerializer, Immutable]
public sealed record KurrentDBRawEvent([property: Id(0)] byte[] Data, [property: Id(1)] byte[] Metadata);
