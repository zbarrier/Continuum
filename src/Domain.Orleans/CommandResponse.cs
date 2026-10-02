namespace Continuum.Domain.Orleans;

/// <summary>The result of saving the uncommitted events of a session.</summary>
/// <param name="Changes">The changes (events) that were saved.</param>
/// <param name="Version">The version of the event stream after the changes were saved.</param>
[GenerateSerializer, Immutable]
public sealed record SaveChangesResponse(IEnumerable<IChange> Changes, int Version) : ISaveChangesResponse
{
    /// <summary>An empty set of changes.</summary>
    public static readonly IEnumerable<IChange> NoChanges = new List<Change>();
}

/// <summary>A single change (event) together with its mapped event type name.</summary>
/// <param name="EventType">The mapped name of the event type.</param>
/// <param name="Event">The event instance.</param>
[GenerateSerializer, Immutable]
public sealed record Change(string EventType, object Event) : IChange;

/// <summary>Base class for responses of commands executed against event-sourced entities.</summary>
[GenerateSerializer, Immutable]
public abstract class EventSourcedCommandResponseBase : IEventSourcedCommandResponse
{
    /// <summary>Initializes a new instance of the <see cref="EventSourcedCommandResponseBase"/> class.</summary>
    /// <param name="changes">The changes (events) produced by the command.</param>
    /// <param name="version">The version of the event stream after the command was applied.</param>
    protected EventSourcedCommandResponseBase(IEnumerable<IChange> changes, int version)
    {
        Changes = changes;
        Version = version;
    }

    /// <inheritdoc/>
    [Id(0)] public IEnumerable<IChange> Changes { get; }

    /// <inheritdoc/>
    [Id(1)] public int Version { get; }
}
