namespace Continuum.Orleans.Events;

/// <summary>
///     What a stream target receives of the transaction its event is appended in.
/// </summary>
public enum StreamTargetScope
{
    /// <summary>
    ///     The whole transaction, so a consumer such as a read model sees every event appended with the one that named
    ///     it, applied at once.
    /// </summary>
    Transaction,

    /// <summary>
    ///     Only the events of the transaction that name the target. Use it when a transaction holds events meant for
    ///     different grains, such as a batch where each event concerns one grain, so a grain is not sent every other
    ///     grain's events.
    /// </summary>
    Event,
}

/// <summary>
///     An additional grain whose stream receives an event, and what it receives of the event's transaction.
/// </summary>
/// <param name="GrainId">The grain whose stream receives the event.</param>
/// <param name="Scope">What the grain's stream receives of the event's transaction.</param>
public readonly record struct StreamTarget(GrainId GrainId, StreamTargetScope Scope = StreamTargetScope.Transaction)
{
    /// <summary>Creates a target that receives the event's whole transaction.</summary>
    /// <param name="grainId">The grain whose stream receives the transaction.</param>
    public static StreamTarget Transaction(GrainId grainId) => new(grainId, StreamTargetScope.Transaction);

    /// <summary>Creates a target that receives only the events of the transaction that name it.</summary>
    /// <param name="grainId">The grain whose stream receives the event.</param>
    public static StreamTarget Event(GrainId grainId) => new(grainId, StreamTargetScope.Event);
}

/// <summary>
///     An event's validated stream targets, as the grain identifiers written to its metadata, by scope.
/// </summary>
/// <param name="TransactionScoped">The targets that receive the event's whole transaction.</param>
/// <param name="EventScoped">The targets that receive only the events of the transaction that name them.</param>
[GenerateSerializer, Immutable, Alias("Continuum.FormattedStreamTargets")]
public sealed record FormattedStreamTargets(
    [property: Id(0)] IReadOnlyList<string> TransactionScoped,
    [property: Id(1)] IReadOnlyList<string> EventScoped);
