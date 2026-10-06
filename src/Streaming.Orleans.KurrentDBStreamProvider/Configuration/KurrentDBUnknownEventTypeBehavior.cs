namespace Orleans.Configuration;

/// <summary>
///     What the KurrentDB stream provider does when it reads an event whose type the type mapper cannot resolve.
/// </summary>
/// <remarks>
///     The choice only matters at delivery. A silo reading <c>$all</c> sees every event on the log, so the type is
///     resolved when an event is handed to a consumer rather than when it is read into the cache, and an unresolvable
///     event therefore affects only the streams someone actually subscribed to.
/// </remarks>
public enum KurrentDBUnknownEventTypeBehavior
{
    /// <summary>
    ///     Fail the delivery, so Orleans retries it and eventually faults the subscription without advancing past the
    ///     event. Use this when every event on a subscribed stream is expected to be readable.
    /// </summary>
    Halt = 0,

    /// <summary>
    ///     Log the event and deliver nothing for it, allowing the subscription to continue past it. Use this only when
    ///     unreadable events on a subscribed stream are expected and safe to ignore.
    /// </summary>
    Skip = 1
}
