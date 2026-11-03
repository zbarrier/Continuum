namespace Continuum.Domain;

/// <summary>A domain event that records who initiated it and when it occurred.</summary>
public interface IDomainEvent
{
    /// <summary>Gets the identifier of the user or system user whose identity initiated the action.</summary>
    Guid InitiatedBy { get; }

    /// <summary>Gets the time at which the event occurred.</summary>
    DateTimeOffset OccurredAt { get; }
}

/// <summary>An event that carries the state machine command (trigger) that produced it.</summary>
/// <typeparam name="TCommand">The state machine trigger type.</typeparam>
public interface IStateMachineEvent<TCommand>
    where TCommand : notnull
{
    /// <summary>Gets the state machine command (trigger) that produced the event.</summary>
    TCommand Command { get; }
}

/// <summary>A domain event that is also a state machine event.</summary>
/// <typeparam name="TCommand">The state machine trigger type.</typeparam>
public interface IStateMachineDomainEvent<TCommand> : IDomainEvent, IStateMachineEvent<TCommand>
    where TCommand : notnull
{ }
