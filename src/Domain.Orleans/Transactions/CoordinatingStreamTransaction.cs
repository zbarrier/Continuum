using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Transactions;

namespace Continuum.Domain.Orleans;

internal sealed class CoordinatingStreamTransaction<TState, TEventBase> : ICoordinatingStreamTransaction<TState, TEventBase>
    where TState : EventDrivenState<TState, TEventBase>, new()
    where TEventBase : class
{
    private readonly IStreamTransaction<TState, TEventBase> _local;

    public CoordinatingStreamTransaction(object owner, IStreamTransaction<TState, TEventBase> local, TransactionCoordination coordination, TimeSpan? timeout)
    {
        Owner = owner;
        _local = local;
        Scope = coordination.Begin(() => local.UncommittedEvents.Count, timeout);
    }

    public object Owner { get; }

    public MultiStreamTransactionScope Scope { get; }

    public Guid TransactionId => Scope.TransactionId;

    public TState State => _local.State;

    public IReadOnlyList<TEventBase> UncommittedEvents => _local.UncommittedEvents;

    public void Apply<TEvent>(TEvent evt) where TEvent : TEventBase => _local.Apply(evt);

    public (TState PreviousState, TState CurrentState) Evolve<TEvent>(TEvent evt) where TEvent : TEventBase => _local.Evolve(evt);

    public Task<Result<PreparedTransaction>> PrepareAsync(ITransactionParticipant participant, object command)
        => Scope.PrepareAsync(participant, command);

    // Rolling back participants needs DisposeAsync; Dispose only releases the local transaction.
    public void Dispose() => _local.Dispose();

    public async ValueTask DisposeAsync()
    {
        await Scope.DisposeAsync();
        _local.Dispose();
    }
}

internal sealed class CoordinatingStateMachineStreamTransaction<TState, TEventBase, TStatus, TCommand>
    : ICoordinatingStateMachineStreamTransaction<TState, TEventBase, TStatus, TCommand>
    where TState : EventDrivenStateMachineState<TState, TEventBase, TStatus, TCommand>, new()
    where TEventBase : StateMachineEvent<TCommand>
    where TStatus : notnull
    where TCommand : notnull
{
    private readonly IStateMachineStreamTransaction<TState, TEventBase, TStatus, TCommand> _local;

    public CoordinatingStateMachineStreamTransaction(object owner, IStateMachineStreamTransaction<TState, TEventBase, TStatus, TCommand> local,
        TransactionCoordination coordination, TimeSpan? timeout)
    {
        Owner = owner;
        _local = local;
        Scope = coordination.Begin(() => local.UncommittedEvents.Count, timeout);
    }

    public object Owner { get; }

    public MultiStreamTransactionScope Scope { get; }

    public Guid TransactionId => Scope.TransactionId;

    public TState State => _local.State;

    public IReadOnlyList<TEventBase> UncommittedEvents => _local.UncommittedEvents;

    public void Apply<TEvent>(TEvent evt) where TEvent : TEventBase => _local.Apply(evt);

    public (TState PreviousState, TState CurrentState) Evolve<TEvent>(TEvent evt) where TEvent : TEventBase => _local.Evolve(evt);

    public Task<Result<PreparedTransaction>> PrepareAsync(ITransactionParticipant participant, object command)
        => Scope.PrepareAsync(participant, command);

    // Rolling back participants needs DisposeAsync; Dispose only releases the local transaction.
    public void Dispose() => _local.Dispose();

    public async ValueTask DisposeAsync()
    {
        await Scope.DisposeAsync();
        _local.Dispose();
    }
}
