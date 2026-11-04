using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Transactions;

namespace Continuum.Domain.Orleans;

/// <summary>
///     A stream transaction run by a grain against its own stream. Events applied through it are applied to the
///     grain's own stream. Participants are prepared as the business logic needs them, and every event is appended
///     atomically, in the order it was generated, when the transaction is saved.
/// </summary>
/// <remarks>
///     Disposing the transaction without a successful save rolls back every participant prepared so far.
/// </remarks>
/// <typeparam name="TState">The state type.</typeparam>
/// <typeparam name="TEventBase">The base type of events applied to the state.</typeparam>
public interface ICoordinatingStreamTransaction<TState, TEventBase> : IStreamTransaction<TState, TEventBase>, IAsyncDisposable
    where TState : IEventDrivenState<TState, TEventBase>, new()
    where TEventBase : class
{
    /// <summary>Gets the ID of the transaction.</summary>
    Guid TransactionId { get; }

    /// <summary>
    ///     Asks <paramref name="participant"/> to run <paramref name="command"/> without saving it. The returned events
    ///     are appended after every event generated so far. A participant may be prepared more than once; each command
    ///     runs on top of the participant's earlier prepared events.
    /// </summary>
    /// <param name="participant">The participant. It must have registered a transaction handler for the command's type.</param>
    /// <param name="command">The command to run.</param>
    /// <returns>The events the command generated and the expected version, or the participant's business error.</returns>
    Task<Result<PreparedTransaction>> PrepareAsync(ITransactionParticipant participant, object command);
}

/// <summary>
///     A stream transaction run by a state machine grain against its own stream. Events applied through it are applied
///     to the grain's own stream. Participants are prepared as the business logic needs them, and every event is
///     appended atomically, in the order it was generated, when the transaction is saved.
/// </summary>
/// <remarks>
///     Disposing the transaction without a successful save rolls back every participant prepared so far.
/// </remarks>
/// <typeparam name="TState">The state type.</typeparam>
/// <typeparam name="TEventBase">The base type of events applied to the state.</typeparam>
/// <typeparam name="TStatus">The state machine status type.</typeparam>
/// <typeparam name="TCommand">The state machine trigger type.</typeparam>
public interface ICoordinatingStateMachineStreamTransaction<TState, TEventBase, TStatus, TCommand>
    : IStateMachineStreamTransaction<TState, TEventBase, TStatus, TCommand>, IAsyncDisposable
    where TState : IEventDrivenStateMachineState<TState, TEventBase, TStatus, TCommand>, new()
    where TEventBase : IStateMachineEvent<TCommand>
    where TStatus : notnull
    where TCommand : notnull
{
    /// <inheritdoc cref="ICoordinatingStreamTransaction{TState, TEventBase}.TransactionId"/>
    Guid TransactionId { get; }

    /// <inheritdoc cref="ICoordinatingStreamTransaction{TState, TEventBase}.PrepareAsync"/>
    Task<Result<PreparedTransaction>> PrepareAsync(ITransactionParticipant participant, object command);
}
