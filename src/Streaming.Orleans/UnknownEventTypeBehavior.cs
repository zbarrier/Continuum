namespace Continuum.Streaming.Orleans;

/// <summary>
///     What a subscription does when it reads an event whose type the type mapper cannot resolve.
/// </summary>
/// <remarks>
///     An unresolvable event is usually a missing type registration rather than a foreign event, so the default is
///     <see cref="Halt" />: skipping it lets the subscription's position advance past an event no consumer ever saw.
/// </remarks>
public enum UnknownEventTypeBehavior
{
    /// <summary>
    ///     Stop at the event without advancing the subscription's position past it. Use this when every event read is
    ///     expected to be readable.
    /// </summary>
    Halt = 0,

    /// <summary>
    ///     Log the event and continue past it. Use this only when unreadable events are expected and safe to ignore.
    /// </summary>
    Skip = 1
}
