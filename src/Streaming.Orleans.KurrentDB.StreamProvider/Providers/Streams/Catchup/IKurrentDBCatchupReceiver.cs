namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Catchup;

/// <summary>
///     A KurrentDB receiver that reads its source with a client managed position, and can therefore ask KurrentDB
///     whether a transaction it is waiting on has more events.
/// </summary>
public interface IKurrentDBCatchupReceiver : IKurrentDBReceiver
{
    /// <summary>
    ///     Reads the source, with the subscription's filter, after <paramref name="afterPosition" /> to find out whether
    ///     the next event belongs to <paramref name="transactionId" />.
    /// </summary>
    /// <param name="transactionId">The transaction waited on.</param>
    /// <param name="afterPosition">The position of the last event received of the transaction.</param>
    /// <param name="cancellationToken">Cancels the read.</param>
    /// <returns>
    ///     <see langword="true" /> when the next event belongs to the transaction, so the rest is still arriving;
    ///     <see langword="false" /> when the next event belongs to something else, or there is none, so the transaction
    ///     is confirmed partial.
    /// </returns>
    /// <remarks>
    ///     A failed or timed out read throws. It confirms nothing, so the caller keeps waiting and verifies again later.
    /// </remarks>
    Task<bool> HasMoreTransactionEventsAsync(Guid transactionId, ulong afterPosition, CancellationToken cancellationToken);
}
