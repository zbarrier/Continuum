namespace Continuum.Domain;

/// <summary>A domain event that records who performed it and when.</summary>
public interface IDomainEvent
{
    /// <summary>Gets the identifier of the user or actor that performed the action.</summary>
    Guid PerformedBy { get; }

    /// <summary>Gets the time at which the action was performed.</summary>
    DateTime PerformedOn { get; }
}

/// <summary>An event that carries the state machine command (trigger) that produced it.</summary>
/// <typeparam name="TCommand">The state machine trigger type.</typeparam>
public interface IStateMachineEvent<TCommand>
    where TCommand : notnull
{
    /// <summary>Gets the state machine command (trigger) that produced the event.</summary>
    TCommand Command { get; init; }
}

/// <summary>A domain event that is also a state machine event.</summary>
/// <typeparam name="TCommand">The state machine trigger type.</typeparam>
public interface IStateMachineDomainEvent<TCommand> : IDomainEvent, IStateMachineEvent<TCommand>
    where TCommand : notnull
{ }
