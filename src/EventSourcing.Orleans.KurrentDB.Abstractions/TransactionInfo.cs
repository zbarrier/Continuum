namespace Continuum.EventSourcing.Orleans.KurrentDB.Abstractions;

/// <summary>
///     Identifies an event's place within the transaction (append) that produced it.
/// </summary>
/// <remarks>
///     The grouping is all or nothing: either every value is present and describes a consistent shape, or the event
///     carries no transaction. KurrentDB reports the same position for every event of an append, so this is the only
///     record of the grouping, and a subscriber uses it to apply the whole append at once.
/// </remarks>
/// <param name="TransactionId">The producer assigned identifier shared by every event of the append.</param>
/// <param name="TransactionSize">The number of events in the append, across every stream it wrote to.</param>
/// <param name="TransactionPartitionSize">
///     The number of the append's events belonging to this event's stream. A consumer of one stream can only ever
///     receive this many, so it is the count such a consumer would group against.
/// </param>
/// <param name="TransactionPartitionIndex">The zero based ordinal of this event within that per stream group.</param>
public readonly record struct TransactionInfo(NewId TransactionId, int TransactionSize, int TransactionPartitionSize, int TransactionPartitionIndex)
{
    /// <summary>
    ///     Gets whether the counts describe a group that can be assembled: at least one event, a partition no larger
    ///     than the transaction, and an index inside the partition.
    /// </summary>
    public bool IsValid =>
        TransactionSize >= 1 &&
        TransactionPartitionSize >= 1 &&
        TransactionPartitionSize <= TransactionSize &&
        TransactionPartitionIndex >= 0 &&
        TransactionPartitionIndex < TransactionPartitionSize;

    /// <summary>
    ///     Throws when the counts do not describe a group that can be assembled.
    /// </summary>
    /// <param name="paramName">The name of the parameter the transaction was passed as.</param>
    /// <exception cref="ArgumentException">The transaction is not <see cref="IsValid"/>.</exception>
    public void EnsureValid(string? paramName = null)
    {
        if (!IsValid)
        {
            throw new ArgumentException(FormattableString.Invariant(
                $"Invalid transaction: size {TransactionSize}, partition size {TransactionPartitionSize}, partition index {TransactionPartitionIndex}. Size and partition size must be at least 1, partition size must not exceed size, and the index must be within the partition."),
                paramName);
        }
    }
}
