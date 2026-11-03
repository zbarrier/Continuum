namespace Continuum.Domain;

/// <summary>The result of persisting the uncommitted events of a session.</summary>
public interface ISessionSaveResponse : IResponse
{
    /// <summary>Gets the changes (events) that were saved.</summary>
    IReadOnlyList<IChange> Changes { get; }

    /// <summary>Gets the version of the event stream after the changes were saved.</summary>
    int Version { get; }
}

/// <summary>A single change (event) together with its mapped event type name.</summary>
public interface IChange
{
    /// <summary>Gets the mapped name of the event type.</summary>
    string EventType { get; }

    /// <summary>Gets the event instance.</summary>
    object Event { get; }
}

/// <summary>The response of a command that includes the changes (events) it produced.</summary>
public interface ICommandResponseWithChanges : IResponse
{
    /// <summary>Gets the changes (events) produced by the command.</summary>
    IReadOnlyList<IChange> Changes { get; }

    /// <summary>Gets the version of the event stream after the command was applied.</summary>
    int Version { get; }
}
