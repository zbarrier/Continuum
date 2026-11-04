using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Transactions;

using Orleans.Concurrency;

namespace Continuum.Domain.Orleans;

/// <summary>A grain that can take part in a multi-stream transaction coordinated by another grain.</summary>
public interface ITransactionParticipant : IGrainWithStringKey
{
    /// <summary>
    ///     Runs <paramref name="command"/> in a session without saving it, and holds the result until the transaction is
    ///     resolved.
    /// </summary>
    /// <param name="transactionId">The coordinator's transaction ID, used to resolve the prepared transaction.</param>
    /// <param name="command">The command to run. The grain must have registered a transaction handler for its type.</param>
    /// <param name="remaining">The time left before the coordinator gives up on the transaction.</param>
    /// <returns>The events to append and the expected version, or the handler's business error.</returns>
    /// <exception cref="NotSupportedException">No transaction handler is registered for the command's type.</exception>
    Task<Result<PreparedTransaction>> PrepareAsync(Guid transactionId, object command, TimeSpan remaining);

    /// <summary>Tells the grain the transaction was committed, so it discards it and refreshes from its stream.</summary>
    /// <param name="transactionId">The transaction ID passed to <see cref="PrepareAsync"/>.</param>
    /// <returns>A task that completes when the call is sent.</returns>
    [OneWay]
    Task CommitAsync(Guid transactionId);

    /// <summary>Tells the grain the transaction was not committed, so it discards it.</summary>
    /// <param name="transactionId">The transaction ID passed to <see cref="PrepareAsync"/>.</param>
    /// <returns>A task that completes when the call is sent.</returns>
    [OneWay]
    Task RollbackAsync(Guid transactionId);
}
