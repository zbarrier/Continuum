namespace Continuum.EventSourcing.Orleans;

/// <summary>
///     Log-consistent storage that can append to the logs of several grains in one atomic transaction.
/// </summary>
/// <remarks>
///     Only stores that commit writes to several streams atomically can implement this. Either every append is
///     committed or none is.
/// </remarks>
public interface IMultiStreamLogConsistentStorage : ILogConsistentStorage
{
    /// <summary>
    ///     Appends entries to the logs of several grains atomically, in the given order.
    /// </summary>
    /// <param name="streams">
    ///     The grains' logs. A grain may appear only once. A stream with no records only checks its expected version.
    ///     The first stream is the grain coordinating the append, which is recorded on every entry for diagnostics.
    /// </param>
    /// <param name="records">
    ///     The entries to append, in the order they were generated across all streams. At least one is required.
    /// </param>
    /// <param name="transactionId">
    ///     The transaction ID to store on every appended entry, which <see cref="ILogConsistentStorage.GetTransactionIdAsync"/>
    ///     returns.
    /// </param>
    /// <typeparam name="TLogEntry">The log entry type.</typeparam>
    /// <returns>The event count of each grain's log after the append, in the order of <paramref name="streams"/>.</returns>
    /// <exception cref="LogAppendConflictException">One or more logs do not have their expected version; nothing was appended.</exception>
    Task<IReadOnlyList<int>> AppendAsync<TLogEntry>(IReadOnlyList<LogStream> streams, IReadOnlyList<LogRecord<TLogEntry>> records, Guid transactionId);
}

/// <summary>
///     One grain's log in a multi-stream append.
/// </summary>
/// <param name="GrainTypeName">The grain type name.</param>
/// <param name="GrainId">The grain ID.</param>
/// <param name="ExpectedVersion">
///     The event count the log must currently have, where 0 means the log must not exist, or <see langword="null"/>
///     to append regardless of the log's version.
/// </param>
public sealed record LogStream(string GrainTypeName, GrainId GrainId, int? ExpectedVersion);

/// <summary>
///     One entry in a multi-stream append.
/// </summary>
/// <param name="StreamIndex">The index of the entry's log in the append's streams.</param>
/// <param name="Entry">The entry to append.</param>
/// <param name="StreamTargets">
///     Additional grain identifiers whose streams should receive the entry's whole transaction, or
///     <see langword="null"/> for none. Already validated by the producer.
/// </param>
/// <typeparam name="TLogEntry">The log entry type.</typeparam>
public sealed record LogRecord<TLogEntry>(int StreamIndex, TLogEntry Entry, IReadOnlyList<string>? StreamTargets = null);
