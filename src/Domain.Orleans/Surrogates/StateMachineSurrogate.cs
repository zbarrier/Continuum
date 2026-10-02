using Stateless;

namespace Continuum.Domain.Orleans;

/// <summary>Orleans serialization surrogate for a <see cref="StateMachine{TState, TTrigger}"/> that captures its current status.</summary>
/// <typeparam name="TStatus">The state machine status type.</typeparam>
/// <typeparam name="TCommand">The state machine trigger type.</typeparam>
[GenerateSerializer, Immutable]
public struct StateMachineSurrogate<TStatus, TCommand>
{
    /// <summary>The current status of the state machine.</summary>
    [Id(0)] public TStatus Status;
}

/// <summary>Converts between a <see cref="StateMachine{TState, TTrigger}"/> and its <see cref="StateMachineSurrogate{TStatus, TCommand}"/>.</summary>
/// <typeparam name="TStatus">The state machine status type.</typeparam>
/// <typeparam name="TCommand">The state machine trigger type.</typeparam>
[RegisterConverter]
public sealed class StateMachineSurrogateConverter<TStatus, TCommand> :
    IConverter<StateMachine<TStatus, TCommand>, StateMachineSurrogate<TStatus, TCommand>>
{
    /// <inheritdoc/>
    public StateMachine<TStatus, TCommand> ConvertFromSurrogate(in StateMachineSurrogate<TStatus, TCommand> surrogate)
    {
        var stateMachine = new StateMachine<TStatus, TCommand>(surrogate.Status, FiringMode.Queued)
        {
            RetainSynchronizationContext = true // Always retain for Orleans
        };

        return stateMachine;
    }

    /// <inheritdoc/>
    public StateMachineSurrogate<TStatus, TCommand> ConvertToSurrogate(in StateMachine<TStatus, TCommand> stateMachine)
    {
        return new StateMachineSurrogate<TStatus, TCommand>()
        {
            Status = stateMachine.State
        };
    }
}
