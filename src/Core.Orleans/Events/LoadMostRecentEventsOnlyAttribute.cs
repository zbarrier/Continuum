namespace Continuum.Orleans.Events;

/// <summary>
///     Makes an event-sourced grain load only its most recent events when it activates, instead of its whole log, for
///     grains whose log grows forever but whose state only needs its latest events (for example, a per-tenant number
///     generator).
/// </summary>
/// <remarks>
///     <para>
///         The grain still loads its snapshot first. When the snapshot is within the most recent <see cref="Count"/>
///         events, the grain applies the events after it as usual. When there is no snapshot, or it is more than
///         <see cref="Count"/> events behind the log, the grain starts from a new state and applies only the most
///         recent <see cref="Count"/> events. The same applies when the grain re-reads its log after a failed write.
///     </para>
///     <para>
///         The grain's version is always the log's real version, so appends, expected-version checks and multi-stream
///         transactions are unaffected. Older events stay in the log and can still be read on demand.
///     </para>
///     <para>
///         The state must be designed for it: applying only the most recent <see cref="Count"/> events must build the
///         state the grain needs, so it must not depend on older events (such as one that created the grain). A state
///         that keeps something from each event should also keep it only for its most recent <see cref="Count"/>
///         events, so a long-lived activation, and its snapshot, hold the same state a fresh activation would load.
///     </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class LoadMostRecentEventsOnlyAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LoadMostRecentEventsOnlyAttribute" /> class.
    /// </summary>
    /// <param name="count">The number of most recent events to load; at least 1.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than 1.</exception>
    public LoadMostRecentEventsOnlyAttribute(int count)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);
        Count = count;
    }

    /// <summary>Gets the number of most recent events the grain loads.</summary>
    public int Count { get; }
}
