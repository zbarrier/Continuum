namespace Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions;

/// <summary>
///     Raised when a catch-up subscription confirms that it only received part of a transaction and
///     <see cref="Continuum.Streaming.Orleans.KurrentDB.Configuration.PartialTransactionHandling.Halt" /> is configured.
/// </summary>
/// <remarks>
///     Nothing of the transaction has been handed to the handler or checkpointed, so it is read again once the
///     subscription filter or the handling is changed and the subscription restarted.
/// </remarks>
public sealed class KurrentDBPartialTransactionException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBPartialTransactionException" /> class.
    /// </summary>
    public KurrentDBPartialTransactionException(Guid transactionId, string streamName, ulong position, int receivedCount, int transactionSize)
        : base($"Transaction {transactionId} in stream {streamName} ending at position {position} was only partially received ({receivedCount} of {transactionSize} events). The subscription is halted; change the subscription filter or the partial transaction handling and restart.")
    {
        TransactionId = transactionId;
        StreamName = streamName;
        Position = position;
        ReceivedCount = receivedCount;
        TransactionSize = transactionSize;
    }

    /// <summary>
    ///     The partially received transaction.
    /// </summary>
    public Guid TransactionId { get; }

    /// <summary>
    ///     The stream whose partition of the transaction was partially received.
    /// </summary>
    public string StreamName { get; }

    /// <summary>
    ///     The position of the last event received of the transaction.
    /// </summary>
    public ulong Position { get; }

    /// <summary>
    ///     The number of events received of the stream's partition of the transaction.
    /// </summary>
    public int ReceivedCount { get; }

    /// <summary>
    ///     The number of events the stream's partition of the transaction was written with.
    /// </summary>
    public int TransactionSize { get; }
}
