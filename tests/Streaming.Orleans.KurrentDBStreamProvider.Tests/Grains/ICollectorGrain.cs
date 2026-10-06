namespace Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests.Grains;

/// <summary>
///     Collects the events delivered to it, so a test can assert on what the stream provider produced.
/// </summary>
public interface ICollectorGrain : IGrainWithGuidKey
{
    /// <summary>
    ///     Subscribes to the given stream, on the <c>$all</c> based stream provider by default.
    /// </summary>
    /// <remarks>
    ///     The two providers see different events: the <c>$all</c> provider is filtered to the event sourced stream
    ///     prefix, so events published through the main provider are only observable through that provider.
    /// </remarks>
    Task Subscribe(StreamId streamId, string? providerName = null);

    /// <summary>
    ///     Returns the messages received so far, in delivery order.
    /// </summary>
    Task<ChatMessage[]> GetReceived();
}
