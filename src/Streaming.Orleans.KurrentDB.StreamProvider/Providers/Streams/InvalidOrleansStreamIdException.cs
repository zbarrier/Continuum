namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider;

/// <summary>
///     Thrown when the Orleans stream id an event carries in its metadata cannot be parsed. It is caught by the cache,
///     which skips the event, and never leaves the provider.
/// </summary>
internal sealed class InvalidOrleansStreamIdException(string orleansStreamId, Exception innerException)
    : Exception($"Orleans stream id '{orleansStreamId}' in event metadata is not a valid stream id.", innerException)
{
    public string OrleansStreamId { get; } = orleansStreamId;
}
