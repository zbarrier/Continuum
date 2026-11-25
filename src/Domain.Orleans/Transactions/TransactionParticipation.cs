using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Events;
using Continuum.Orleans.Transactions;

namespace Continuum.Domain.Orleans;

/// <summary>Holds a grain's transaction handlers and its prepared transactions until they are resolved.</summary>
/// <typeparam name="TSession">The grain's session type.</typeparam>
internal sealed class TransactionParticipation<TSession>
    where TSession : IDisposable
{
    /// <summary>Added to the coordinator's remaining time so the coordinator always gives up first.</summary>
    internal static readonly TimeSpan TimeoutBuffer = TimeSpan.FromSeconds(5);

    private readonly Dictionary<Type, (Func<object, TSession, Task<Result>> Handler, bool CheckVersion)> _handlers = new();
    private readonly Dictionary<Guid, Prepared> _prepared = new();
    private readonly Grain _grain;
    private readonly Func<TSession> _createSession;
    private readonly Func<TSession, IReadOnlyList<object>> _getEvents;
    private readonly Func<int> _getVersion;
    private readonly Func<Task> _refresh;
    private readonly Func<object, FormattedStreamTargets?> _getStreamTargets;

    public TransactionParticipation(Grain grain, Func<TSession> createSession, Func<TSession, IReadOnlyList<object>> getEvents,
        Func<int> getVersion, Func<Task> refresh, Func<object, FormattedStreamTargets?> getStreamTargets)
    {
        _grain = grain;
        _createSession = createSession;
        _getEvents = getEvents;
        _getVersion = getVersion;
        _refresh = refresh;
        _getStreamTargets = getStreamTargets;
    }

    public void Register<TCommand>(Func<TCommand, TSession, Task<Result>> handler, bool checkVersion)
    {
        ArgumentNullException.ThrowIfNull(handler);
        if (!_handlers.TryAdd(typeof(TCommand), ((command, session) => handler((TCommand)command, session), checkVersion)))
        {
            throw new InvalidOperationException("Duplicate transaction command handler.");
        }
    }

    // A second prepare for the same transaction runs against the session of the first, so it sees the events already
    // prepared, and returns only its own new events. The expected version is the one captured by the first prepare.
    public async Task<Result<PreparedTransaction>> PrepareAsync(Guid transactionId, object command, TimeSpan remaining)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(remaining, TimeSpan.Zero);
        if (!_handlers.TryGetValue(command.GetType(), out var registration))
        {
            throw new NotSupportedException($"{_grain.GetType().Name} does not take part in transactions for {command.GetType().Name}.");
        }

        if (!_prepared.TryGetValue(transactionId, out var prepared))
        {
            prepared = new Prepared(_createSession(), _getVersion());
            _prepared[transactionId] = prepared;
        }
        else if (prepared.Faulted)
        {
            throw new InvalidOperationException("An earlier prepare for this transaction failed, so its session can no longer be used.");
        }
        prepared.Timer?.Dispose();
        prepared.Timer = _grain.RegisterGrainTimer(
            _ => OnTimeoutAsync(transactionId), remaining + TimeoutBuffer, Timeout.InfiniteTimeSpan);

        var eventCount = _getEvents(prepared.Session).Count;
        Result result;
        try
        {
            result = await registration.Handler(command, prepared.Session);
        }
        catch
        {
            prepared.Faulted = true;
            throw;
        }
        var events = _getEvents(prepared.Session);
        if (result.IsFailure)
        {
            // The handler may have applied events before failing, so the session is no longer trustworthy.
            prepared.Faulted = events.Count != eventCount;
            return Result.Failure<PreparedTransaction>(result.Error);
        }

        var newEvents = events.Skip(eventCount).ToArray();
        IReadOnlyList<FormattedStreamTargets?>? targets;
        try
        {
            targets = GetStreamTargets(newEvents);
        }
        catch
        {
            // The session holds events the coordinator will never receive.
            prepared.Faulted = true;
            throw;
        }
        return Result.Success(new PreparedTransaction(
            _grain.GetGrainId(),
            newEvents,
            registration.CheckVersion ? prepared.Version : null,
            targets));
    }

    // The owner of an event chooses its targets, so they are the same whether the grain saves the event itself or
    // prepares it for another grain's transaction.
    private FormattedStreamTargets?[]? GetStreamTargets(object[] events)
    {
        var targets = new FormattedStreamTargets?[events.Length];
        var any = false;
        for (var index = 0; index < events.Length; index++)
        {
            targets[index] = _getStreamTargets(events[index]);
            any |= targets[index] is not null;
        }
        return any ? targets : null;
    }

    // Always refreshes, even for an unknown ID (the grain may have been reactivated since it prepared). The refresh
    // only reads events past the confirmed version.
    public Task CommitAsync(Guid transactionId)
    {
        Discard(transactionId);
        return _refresh();
    }

    public Task RollbackAsync(Guid transactionId)
    {
        Discard(transactionId);
        return Task.CompletedTask;
    }

    // Neither CommitAsync nor RollbackAsync arrived. The transaction may have committed anyway, so refresh; the
    // refresh only reads events past the confirmed version.
    private Task OnTimeoutAsync(Guid transactionId)
        => Discard(transactionId) ? _refresh() : Task.CompletedTask;

    private bool Discard(Guid transactionId)
    {
        if (!_prepared.Remove(transactionId, out var prepared))
        {
            return false;
        }
        prepared.Timer?.Dispose();
        prepared.Session.Dispose();
        return true;
    }

    private sealed class Prepared(TSession session, int version)
    {
        public TSession Session { get; } = session;

        public int Version { get; } = version;

        public IGrainTimer? Timer { get; set; }

        public bool Faulted { get; set; }
    }
}
