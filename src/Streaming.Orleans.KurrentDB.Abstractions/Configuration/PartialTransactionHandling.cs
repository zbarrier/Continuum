namespace Continuum.Streaming.Orleans.KurrentDB.Configuration;

/// <summary>
///     What a KurrentDB catch-up subscription does with a transaction it only received part of.
/// </summary>
/// <remarks>
///     <para>
///         A transaction is written in one atomic append, so its events are contiguous in <c>$all</c> and all become
///         visible together. A transaction stays partial when the subscription filter excludes some of its events. That
///         is intended by some filters and a mistake in others, and it is the same every time the transaction is read.
///     </para>
///     <para>
///         A transaction is only treated as partial once KurrentDB confirms it: an event of another transaction
///         arrived after it, or a verification read found no further event of it. A dropped or slow connection never
///         confirms anything, so it can only delay the decision. Partial transactions are never dropped.
///     </para>
/// </remarks>
public enum PartialTransactionHandling
{
    /// <summary>
    ///     Halt the subscription with a critical log. Nothing past the transaction is delivered or checkpointed until
    ///     the filter is fixed, or the handling changed, and the subscription restarted.
    /// </summary>
    Halt = 0,

    /// <summary>
    ///     Deliver the events received as the transaction and log a warning.
    /// </summary>
    DeliverAndWarn = 1,

    /// <summary>
    ///     Deliver the events received as the transaction and log at debug level. Use when the filter is meant to
    ///     exclude part of some transactions.
    /// </summary>
    Deliver = 2,
}
