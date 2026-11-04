using Continuum.CSharpFunctionalExtensions;
using Continuum.Errors;
using Continuum.Orleans.Transactions;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Orleans.EventSourcing;
using Orleans.Serialization;

namespace Continuum.Domain.Orleans;

/// <summary>A grain whose state machine backed state is built from events.</summary>
public interface IEventDrivenStateMachineGrain : IEventLogGrain;

/// <summary>A journaled grain whose <see cref="EventDrivenStateMachineState{TState, TEventBase, TStatus, TCommand}"/> is changed through sessions.</summary>
/// <typeparam name="TState">The state type.</typeparam>
/// <typeparam name="TEventBase">The base type of events applied to the state.</typeparam>
/// <typeparam name="TStatus">The state machine status type.</typeparam>
/// <typeparam name="TCommand">The state machine trigger type.</typeparam>
public abstract class EventDrivenStateMachineGrain<TState, TEventBase, TStatus, TCommand> : JournaledGrain<TState, TEventBase>,
    IEventDrivenStateMachineGrain,
    ITransactionParticipant,
    ITransactionSubmissionSource
    where TState : EventDrivenStateMachineState<TState, TEventBase, TStatus, TCommand>, new()
    where TEventBase : StateMachineEvent<TCommand>
    where TStatus : notnull
    where TCommand : notnull
{
    /// <summary>The error returned when events cannot be saved because of an optimistic concurrency conflict.</summary>
    protected static readonly Error ConcurrencyConflictError = ContinuumErrors.NewOptimisticConcurrencyConflict();

    /// <summary>The copier used to isolate session state from grain state.</summary>
    protected readonly DeepCopier<TState> _deepCopier;

    /// <summary>The type mapper used to name events.</summary>
    protected readonly ITypeMapper _typeMapper;


    private readonly TransactionParticipation<IStateMachineStreamTransaction<TState, TEventBase, TStatus, TCommand>> _transactions;
    private readonly TransactionCoordination _coordination = new();

    /// <inheritdoc/>
    TransactionSubmission? ITransactionSubmissionSource.PendingTransaction => _coordination.PendingTransaction;

    /// <summary>Gets the grain's logger.</summary>
    public ILogger Logger { get; }


    /// <summary>Initializes a new instance of the <see cref="EventDrivenStateMachineGrain{TState, TEventBase, TStatus, TCommand}"/> class.</summary>
    /// <param name="serviceProvider">The service provider used to resolve the deep copier and type mapper.</param>
    /// <param name="connectionName">The service key of the <see cref="ITypeMapper"/> to use.</param>
    protected EventDrivenStateMachineGrain(IServiceProvider serviceProvider, object connectionName)
    {
        _deepCopier = serviceProvider.GetRequiredService<DeepCopier<TState>>();
        _typeMapper = serviceProvider.GetRequiredKeyedService<ITypeMapper>(connectionName);
        Logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());
        _transactions = new TransactionParticipation<IStateMachineStreamTransaction<TState, TEventBase, TStatus, TCommand>>(
            this, CreateParticipantTransaction, transaction => transaction.UncommittedEvents, () => Version, RefreshNow);
    }

    /// <summary>
    /// FOR TESTING ONLY!!! Used for Given, When, Then tests.
    /// </summary>
    /// <param name="events">The given events used to setup the initial state.</param>
    /// <exception cref="InvalidOperationException">Thrown if applying the events fails.</exception>
    public async Task LoadGivenEvents(params object[] events)
    {
        var typedEvents = new List<TEventBase>();
        foreach (var evt in events)
        {
            if (evt is TEventBase typedEvent)
            {
                typedEvents.Add(typedEvent);
            }
            else
            {
                throw new ArgumentException($"Event must be of type {typeof(TEventBase).Name}", nameof(events));
            }
        }
        var success = await RaiseConditionalEvents(typedEvents);
        if (!success)
        {
            throw new InvalidOperationException("Failed to apply given events.");
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetSavedVersion()
    {
        await RefreshNow();
        return Version;
    }

    /// <inheritdoc/>
    public async Task<object[]> GetSavedEventsSince(int version)
    {
        await RefreshNow();
        var events = await RetrieveConfirmedEvents(version, Version);
        return events.Cast<object>().ToArray();
    }

    private IStateMachineStreamTransaction<TState, TEventBase, TStatus, TCommand> CreateParticipantTransaction()
        => new EventDrivenStateMachineSession<TState, TEventBase, TStatus, TCommand>(_deepCopier, State);

    /// <summary>
    ///     Begins a stream transaction over a copy of the grain's state. Events applied through it go to the grain's own
    ///     stream, and participants can be prepared through it. Save it with
    ///     <see cref="SaveChanges(ICoordinatingStateMachineStreamTransaction{TState, TEventBase, TStatus, TCommand})"/>.
    ///     Disposing it without a successful save rolls back every participant prepared so far.
    /// </summary>
    /// <param name="timeout">How long the transaction may take when participants are prepared; defaults to 30 seconds.</param>
    /// <returns>The transaction.</returns>
    protected internal ICoordinatingStateMachineStreamTransaction<TState, TEventBase, TStatus, TCommand> BeginStreamTransaction(TimeSpan? timeout = null)
        => new CoordinatingStateMachineStreamTransaction<TState, TEventBase, TStatus, TCommand>(this, CreateParticipantTransaction(), _coordination, timeout);

    /// <summary>
    ///     Saves the transaction's events. When participants were prepared, their events are saved together with the
    ///     grain's own events in one atomic append, in the order they were generated, then the participants are told the
    ///     transaction was committed.
    /// </summary>
    /// <param name="transaction">A transaction begun by this grain.</param>
    /// <returns>The save response, or a concurrency conflict.</returns>
    /// <exception cref="ArgumentException"><paramref name="transaction"/> was not begun by this grain.</exception>
    protected internal async Task<Result<IStreamTransactionResponse>> SaveChanges(ICoordinatingStateMachineStreamTransaction<TState, TEventBase, TStatus, TCommand> transaction)
    {
        if (transaction is not CoordinatingStateMachineStreamTransaction<TState, TEventBase, TStatus, TCommand> coordinating || !ReferenceEquals(coordinating.Owner, this))
        {
            throw new ArgumentException("The transaction was not begun by this grain.", nameof(transaction));
        }

        var result = await _coordination.SaveAsync(coordinating.Scope, transaction.UncommittedEvents, RaiseConditionalEvents);
        if (result.IsFailure)
        {
            return Result.Failure<IStreamTransactionResponse>(result.Error);
        }
        if (transaction.UncommittedEvents.Count == 0)
        {
            IStreamTransactionResponse noChangesResponse = new StreamTransactionResponse(StreamTransactionResponse.NoChanges, Version);
            return Result.Success(noChangesResponse);
        }

        var changes = new List<IChange>(transaction.UncommittedEvents.Count);
        foreach (var evt in transaction.UncommittedEvents)
        {
            changes.Add(new Change(_typeMapper.GetTypeName(evt.GetType()), evt));
        }
        IStreamTransactionResponse changesResponse = new StreamTransactionResponse(changes, Version);
        return Result.Success(changesResponse);
    }

    /// <summary>Lets commands of type <typeparamref name="TTransactionCommand"/> take part in transactions coordinated by another grain.</summary>
    /// <typeparam name="TTransactionCommand">The command type.</typeparam>
    /// <param name="handler">Runs the command against the session. Its events are returned to the coordinator, not saved.</param>
    /// <param name="checkVersion">Whether the append must fail if this grain's log changed after it prepared.</param>
    /// <exception cref="InvalidOperationException">A handler is already registered for <typeparamref name="TTransactionCommand"/>.</exception>
    protected void OnTransaction<TTransactionCommand>(Func<TTransactionCommand, IStateMachineStreamTransaction<TState, TEventBase, TStatus, TCommand>, Task<Result>> handler, bool checkVersion = true)
        => _transactions.Register(handler, checkVersion);

    /// <inheritdoc/>
    public Task<Result<PreparedTransaction>> PrepareAsync(Guid transactionId, object command, TimeSpan remaining)
        => _transactions.PrepareAsync(transactionId, command, remaining);

    /// <inheritdoc/>
    public Task CommitAsync(Guid transactionId) => _transactions.CommitAsync(transactionId);

    /// <inheritdoc/>
    public Task RollbackAsync(Guid transactionId) => _transactions.RollbackAsync(transactionId);


    /// <inheritdoc/>
    protected override void TransitionState(TState state, TEventBase evt)
    {
        if (state == TentativeState)
        {
            return;
        }
        state.When(evt);
    }
}
