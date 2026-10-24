namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider;

/// <summary>
///     The undeserialized payload of a KurrentDB event as it was read from the log.
/// </summary>
/// <param name="Data">
///     The raw event body. This provider writes an Orleans serialized batch here, so it stays opaque until a consumer
///     reads it and the batch container materializes it.
/// </param>
/// <remarks>
///     The payload is kept as bytes rather than a deserialized object so it can be copied straight into the cache
///     buffers. Deserializing in the receiver would materialize every event, including the ones no consumer ever reads.
/// </remarks>
[GenerateSerializer, Immutable]
public sealed record KurrentDBRawEvent([property: Id(0)] byte[] Data);
