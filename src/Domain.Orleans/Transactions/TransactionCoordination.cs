using Continuum.CSharpFunctionalExtensions;
using Continuum.Errors;
using Continuum.Orleans.Transactions;

namespace Continuum.Domain.Orleans;

/// <summary>Runs a coordinator grain's side of its multi-stream transactions.</summary>
internal sealed class TransactionCoordination : ITransactionSubmissionSource
{
    /// <summary>The time a transaction may take when the caller does not give one.</summary>
    internal static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    private static readonly Error ConflictError = ContinuumErrors.NewOptimisticConcurrencyConflict();

    /// <inheritdoc/>
    public TransactionSubmission? PendingTransaction { get; private set; }

    public MultiStreamTransactionScope Begin(Func<int> getCoordinatorEventCount, TimeSpan? timeout)
        => new MultiStreamTransactionScope(getCoordinatorEventCount, timeout ?? DefaultTimeout);

    /// <summary>
    ///     Submits the coordinator's events together with the participants' events, then commits the participants.
    ///     On failure the participants are left for the scope to roll back when it is disposed.
    /// </summary>
    /// <param name="scope">The transaction scope.</param>
    /// <param name="events">The coordinator's events.</param>
    /// <param name="raise">Submits the coordinator's events; it must submit them synchronously, as Orleans does.</param>
    /// <returns>Success, or a concurrency conflict.</returns>
    public async Task<Result> SaveAsync<TEvent>(MultiStreamTransactionScope scope, IReadOnlyList<TEvent> events,
        Func<IReadOnlyList<TEvent>, Task<bool>> raise)
    {
        var submission = scope.CreateSubmission();
        if (!scope.HasParticipants)
        {
            // Nothing to coordinate; this is a normal save.
            var saved = events.Count == 0 || await raise(events);
            if (!saved)
            {
                return Result.Failure(ConflictError);
            }
            await scope.CommitAsync();
            return Result.Success();
        }
        if (events.Count == 0)
        {
            // The coordinator's own stream records the transaction ID used to settle an unknown append outcome.
            throw new InvalidOperationException("The coordinator must raise at least one event in a transaction.");
        }

        Task<bool> appendTask;
        PendingTransaction = submission;
        try
        {
            appendTask = raise(events);
        }
        finally
        {
            PendingTransaction = null;
        }

        var appended = await appendTask;
        if (!appended || submission.IsConflict)
        {
            return Result.Failure(ConflictError);
        }
        if (submission.Failure is not null)
        {
            throw new InvalidOperationException("The transaction could not be appended.", submission.Failure);
        }

        await scope.CommitAsync();
        return Result.Success();
    }
}
