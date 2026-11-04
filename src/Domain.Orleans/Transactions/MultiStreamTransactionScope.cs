using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Transactions;

namespace Continuum.Domain.Orleans;

/// <summary>Tracks a coordinator's transaction: its participants and the order every event was generated in.</summary>
internal sealed class MultiStreamTransactionScope : IAsyncDisposable
{
    private readonly Func<int> _getCoordinatorEventCount;
    private readonly DateTimeOffset _deadline;
    private readonly List<ITransactionParticipant> _touched = new();
    private readonly Dictionary<GrainId, int> _participantIndexes = new();
    private readonly List<TransactionParticipantLog> _participants = new();
    private readonly List<TransactionRecord> _records = new();
    private int _coordinatorEventCount;
    private bool _resolved;

    public MultiStreamTransactionScope(Func<int> getCoordinatorEventCount, TimeSpan timeout)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeout, TimeSpan.Zero);
        _getCoordinatorEventCount = getCoordinatorEventCount;
        // Events already in the session were generated before the transaction began.
        _coordinatorEventCount = getCoordinatorEventCount();
        AddCoordinatorRecords(_coordinatorEventCount);
        _deadline = DateTimeOffset.UtcNow + timeout;
    }

    /// <inheritdoc/>
    public Guid TransactionId { get; } = Guid.CreateVersion7();

    /// <summary>Gets whether any participant has events or a version to check.</summary>
    public bool HasParticipants => _participants.Count > 0;

    /// <inheritdoc/>
    public async Task<Result<PreparedTransaction>> PrepareAsync(ITransactionParticipant participant, object command)
    {
        ArgumentNullException.ThrowIfNull(participant);
        ArgumentNullException.ThrowIfNull(command);
        ObjectDisposedException.ThrowIf(_resolved, this);
        var remaining = _deadline - DateTimeOffset.UtcNow;
        if (remaining <= TimeSpan.Zero)
        {
            throw new TimeoutException("The transaction timed out.");
        }

        // The coordinator's events generated since the last prepare come before this participant's events.
        SyncCoordinatorEvents();
        if (!_touched.Contains(participant))
        {
            _touched.Add(participant);
        }
        var result = await participant.PrepareAsync(TransactionId, command, remaining);
        if (result.IsFailure)
        {
            return result;
        }

        var prepared = result.Value;
        if (!_participantIndexes.TryGetValue(prepared.GrainId, out var index))
        {
            index = _participants.Count;
            _participantIndexes[prepared.GrainId] = index;
            _participants.Add(new TransactionParticipantLog(prepared.GrainId, prepared.ExpectedVersion));
        }
        foreach (var evt in prepared.Events)
        {
            _records.Add(new TransactionRecord(index, evt));
        }
        return result;
    }

    /// <summary>Builds the submission once the coordinator's logic is done.</summary>
    public TransactionSubmission CreateSubmission()
    {
        ObjectDisposedException.ThrowIf(_resolved, this);
        SyncCoordinatorEvents();
        return new TransactionSubmission(TransactionId, _participants.ToArray(), _records.ToArray());
    }

    /// <summary>Tells every participant the transaction was committed.</summary>
    public Task CommitAsync()
    {
        _resolved = true;
        return Task.WhenAll(_touched.Select(p => p.CommitAsync(TransactionId)));
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_resolved)
        {
            return;
        }
        _resolved = true;
        await Task.WhenAll(_touched.Select(p => p.RollbackAsync(TransactionId)));
    }

    private void SyncCoordinatorEvents()
    {
        var count = _getCoordinatorEventCount();
        if (count < _coordinatorEventCount)
        {
            throw new InvalidOperationException("The coordinator's session lost events during the transaction.");
        }
        AddCoordinatorRecords(count - _coordinatorEventCount);
        _coordinatorEventCount = count;
    }

    private void AddCoordinatorRecords(int count)
    {
        for (var i = 0; i < count; i++)
        {
            _records.Add(new TransactionRecord(null, null));
        }
    }
}
