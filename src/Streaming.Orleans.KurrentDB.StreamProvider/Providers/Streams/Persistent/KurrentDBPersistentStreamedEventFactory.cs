using Continuum.Streaming.Orleans.KurrentDB.Extensions;

using KurrentDB.Client;

using Continuum.Streams;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Persistent;

/// <summary>
///     Builds the <see cref="StreamedEvent{T}" /> carriers a persistent subscription receiver hands to the queue cache.
/// </summary>
internal static class KurrentDBPersistentStreamedEventFactory
{
    /// <summary>
    ///     Creates a carrier for an event delivered by a persistent subscription.
    /// </summary>
    /// <param name="resolvedEvent">The event as delivered by the subscription.</param>
    /// <param name="sourceConnectionName">The logical connection the event was read from.</param>
    /// <param name="nameParser">The parser for the event's stream name, or <see langword="null" /> for the default.</param>
    /// <remarks>
    ///     A persistent subscription tracks progress on the server, so the commit position is only informational here.
    ///     It is still populated when the server reports one so the carrier means the same thing for both strategies.
    /// </remarks>
    public static StreamedEvent<KurrentDBRawEvent> FromPersistentSubscription(ResolvedEvent resolvedEvent, string sourceConnectionName, IStreamNameParser? nameParser = null)
    {
        var commitPosition = (resolvedEvent.OriginalPosition ?? resolvedEvent.Event.Position).CommitPosition;
        return KurrentDBStreamedEventFactory.Create(resolvedEvent.Event, sourceConnectionName, commitPosition, nameParser);
    }
}
