namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Catchup;

/// <summary>
///     Raised when a transaction does not fit in one cache block, which halts the catch-up subscription rather than
///     skipping the transaction.
/// </summary>
/// <remarks>
///     A transaction is cached as one record per stream, so the whole transaction must fit in a single block. Raise the
///     cache block size to at least <see cref="Size" /> bytes and restart the silo; the checkpoint has not moved past
///     the transaction, so it is read again.
/// </remarks>
public sealed class KurrentDBTransactionTooLargeForCacheException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBTransactionTooLargeForCacheException" /> class.
    /// </summary>
    public KurrentDBTransactionTooLargeForCacheException(Guid? transactionId, ulong commitPosition, int size, int blockSize, Exception? innerException = null)
        : base($"Transaction {transactionId} ending at commit position {commitPosition} needs {size} bytes in the cache, but the cache block size is {blockSize} bytes. Raise the cache block size to at least {size} bytes and restart.", innerException)
    {
        TransactionId = transactionId;
        CommitPosition = commitPosition;
        Size = size;
        BlockSize = blockSize;
    }

    /// <summary>
    ///     The transaction that did not fit, or <see langword="null" /> when the event was not written in a transaction.
    /// </summary>
    public Guid? TransactionId { get; }

    /// <summary>
    ///     The commit position of the transaction's last event.
    /// </summary>
    public ulong CommitPosition { get; }

    /// <summary>
    ///     The number of bytes the transaction needs in the cache, the minimum block size that covers it.
    /// </summary>
    public int Size { get; }

    /// <summary>
    ///     The configured cache block size, in bytes.
    /// </summary>
    public int BlockSize { get; }
}
