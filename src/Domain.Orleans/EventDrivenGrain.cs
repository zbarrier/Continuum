using Continuum.CSharpFunctionalExtensions;
using Continuum.Errors;
using Continuum.Orleans.Transactions;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Orleans.EventSourcing;
using Orleans.Serialization;

namespace Continuum.Domain.Orleans;

/// <summary>A grain whose state is built from a saved event log.</summary>
public interface IEventLogGrain : IGrainWithStringKey
{
    /// <summary>FOR TESTING ONLY. Applies the given events to set up the initial state for Given/When/Then tests.</summary>
    /// <param name="events">The given events used to set up the initial state.</param>
    /// <returns>A task that completes when the events have been applied.</returns>
    Task LoadGivenEvents(params object[] events);

    /// <summary>FOR TESTING ONLY. Gets the version of the grain's saved event log for Given/When/Then tests.</summary>
    /// <returns>The saved version, after reading any events saved by others.</returns>
    Task<int> GetSavedVersion();

    /// <summary>FOR TESTING ONLY. Gets the saved events after a version for Given/When/Then tests.</summary>
    /// <param name="version">The version to read after.</param>
    /// <returns>The saved events after <paramref name="version"/>, after reading any events saved by others.</returns>
    Task<object[]> GetSavedEventsSince(int version);
}

/// <summary>A grain whose state is built from events.</summary>
public interface IEventDrivenGrain : IEventLogGrain;

/// <summary>An <see cref="EventDrivenGrain{TState, TEventBase}"/> whose events derive from <see cref="object"/>.</summary>
/// <typeparam name="TState">The state type.</typeparam>
public abstract class EventDrivenGrain<TState> : EventDrivenGrain<TState, object>
    where TState : EventDrivenState<TState, object>, new()
{
    /// <summary>Initializes a new instance of the <see cref="EventDrivenGrain{TState}"/> class.</summary>
    /// <param name="serviceProvider">The service provider used to resolve the deep copier and type mapper.</param>
    /// <param name="connectionName">The service key of the <see cref="ITypeMapper"/> to use.</param>
    protected EventDrivenGrain(IServiceProvider serviceProvider, object connectionName) 
        : base(serviceProvider, connectionName) 
    { }
}

/// <summary>A journaled grain whose <see cref="EventDrivenState{TState, TEventBase}"/> is changed through sessions.</summary>
/// <typeparam name="TState">The state type.</typeparam>
/// <typeparam name="TEventBase">The base type of events applied to the state.</typeparam>
public abstract class EventDrivenGrain<TState, TEventBase> : JournaledGrain<TState, TEventBase>,
    IEventDrivenGrain,
    ITransactionParticipant,
    ITransactionSubmissionSource
    where TState : EventDrivenState<TState, TEventBase>, new()
    where TEventBase : class
{
    /// <summary>The error returned when events cannot be saved because of an optimistic concurrency conflict.</summary>
    protected static readonly Error ConcurrencyConflictError = ContinuumErrors.NewOptimisticConcurrencyConflict();

    /// <summary>The copier used to isolate session state from grain state.</summary>
    protected readonly DeepCopier<TState> _deepCopier;

    /// <summary>The type mapper used to name events.</summary>
    protected readonly ITypeMapper _typeMapper;

    private readonly TransactionParticipation<IStreamTransaction<TState, TEventBase>> _transactions;
    private readonly TransactionCoordination _coordination = new();

    /// <inheritdoc/>
    TransactionSubmission? ITransactionSubmissionSource.PendingTransaction => _coordination.PendingTransaction;

    /// <summary>Gets the grain's logger.</summary>
    public ILogger Logger { get; }


    /// <summary>Initializes a new instance of the <see cref="EventDrivenGrain{TState, TEventBase}"/> class.</summary>
    /// <param name="serviceProvider">The service provider used to resolve the deep copier and type mapper.</param>
    /// <param name="connectionName">The service key of the <see cref="ITypeMapper"/> to use.</param>
    protected EventDrivenGrain(IServiceProvider serviceProvider, object connectionName) 
    {
        _deepCopier = serviceProvider.GetRequiredService<DeepCopier<TState>>(); //deepCopier;
        _typeMapper = serviceProvider.GetRequiredKeyedService<ITypeMapper>(connectionName); //typeMapper;
        Logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());
        _transactions = new TransactionParticipation<IStreamTransaction<TState, TEventBase>>(
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

    private IStreamTransaction<TState, TEventBase> CreateParticipantTransaction()
        => new EventDrivenSession<TState, TEventBase>(_deepCopier, State);

    /// <summary>
    ///     Begins a stream transaction over a copy of the grain's state. Events applied through it go to the grain's own
    ///     stream, and participants can be prepared through it. Save it with
    ///     <see cref="SaveChanges(ICoordinatingStreamTransaction{TState, TEventBase})"/>. Disposing it without a
    ///     successful save rolls back every participant prepared so far.
    /// </summary>
    /// <param name="timeout">How long the transaction may take when participants are prepared; defaults to 30 seconds.</param>
    /// <returns>The transaction.</returns>
    protected internal ICoordinatingStreamTransaction<TState, TEventBase> BeginStreamTransaction(TimeSpan? timeout = null)
        => new CoordinatingStreamTransaction<TState, TEventBase>(this, CreateParticipantTransaction(), _coordination, timeout);

    /// <summary>
    ///     Saves the transaction's events. When participants were prepared, their events are saved together with the
    ///     grain's own events in one atomic append, in the order they were generated, then the participants are told the
    ///     transaction was committed.
    /// </summary>
    /// <param name="transaction">A transaction begun by this grain.</param>
    /// <returns>The save response, or a concurrency conflict.</returns>
    /// <exception cref="ArgumentException"><paramref name="transaction"/> was not begun by this grain.</exception>
    protected internal async Task<Result<IStreamTransactionResponse>> SaveChanges(ICoordinatingStreamTransaction<TState, TEventBase> transaction)
    {
        if (transaction is not CoordinatingStreamTransaction<TState, TEventBase> coordinating || !ReferenceEquals(coordinating.Owner, this))
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

    /// <summary>Lets commands of type <typeparamref name="TCommand"/> take part in transactions coordinated by another grain.</summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <param name="handler">Runs the command against the session. Its events are returned to the coordinator, not saved.</param>
    /// <param name="checkVersion">Whether the append must fail if this grain's log changed after it prepared.</param>
    /// <exception cref="InvalidOperationException">A handler is already registered for <typeparamref name="TCommand"/>.</exception>
    protected void OnTransaction<TCommand>(Func<TCommand, IStreamTransaction<TState, TEventBase>, Task<Result>> handler, bool checkVersion = true)
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
