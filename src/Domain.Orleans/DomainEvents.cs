namespace Continuum.Domain.Orleans;

/// <summary>Base class for domain events that record who initiated them and when they occurred.</summary>
/// <remarks>Marked <see cref="ImmutableAttribute"/>: domain events must always be immutable, so derived events must not expose mutable state.</remarks>
[GenerateSerializer, Immutable]
public abstract class DomainEvent : IDomainEvent
{
    /// <summary>Initializes a new instance of the <see cref="DomainEvent"/> class.</summary>
    /// <param name="initiatedBy">The identifier of the user or system user whose identity initiated the action.</param>
    /// <param name="occurredAt">The time at which the event occurred.</param>
    public DomainEvent(Guid initiatedBy, DateTimeOffset occurredAt)
    {
        InitiatedBy = initiatedBy;
        OccurredAt = occurredAt;
    }

    /// <inheritdoc/>
    [Id(0)]
    public Guid InitiatedBy { get; init; }

    /// <inheritdoc/>
    [Id(1)]
    public DateTimeOffset OccurredAt { get; init; }
}

/// <summary>Base class for events that carry the state machine command (trigger) that produced them.</summary>
/// <remarks>Marked <see cref="ImmutableAttribute"/>: domain events must always be immutable, so derived events must not expose mutable state.</remarks>
/// <typeparam name="TCommand">The state machine trigger type.</typeparam>
[GenerateSerializer, Immutable]
public abstract class StateMachineEvent<TCommand> : IStateMachineEvent<TCommand>
    where TCommand : notnull
{
    /// <summary>Initializes a new instance of the <see cref="StateMachineEvent{TCommand}"/> class.</summary>
    /// <param name="command">The state machine command (trigger) that produced the event.</param>
    public StateMachineEvent(TCommand command)
    {
        Command = command;
    }

    /// <inheritdoc/>
    [Id(0)]
    public TCommand Command { get; init; }
}

/// <summary>Base class for state machine events that are also domain events.</summary>
/// <remarks>Marked <see cref="ImmutableAttribute"/>: domain events must always be immutable, so derived events must not expose mutable state.</remarks>
/// <typeparam name="TCommand">The state machine trigger type.</typeparam>
[GenerateSerializer, Immutable]
public abstract class StateMachineDomainEvent<TCommand> : StateMachineEvent<TCommand>, IDomainEvent
    where TCommand : notnull
{
    /// <summary>Initializes a new instance of the <see cref="StateMachineDomainEvent{TCommand}"/> class.</summary>
    /// <param name="command">The state machine command (trigger) that produced the event.</param>
    /// <param name="initiatedBy">The identifier of the user or system user whose identity initiated the action.</param>
    /// <param name="occurredAt">The time at which the event occurred.</param>
    public StateMachineDomainEvent(TCommand command, Guid initiatedBy, DateTimeOffset occurredAt)
        : base(command)
    {
        InitiatedBy = initiatedBy;
        OccurredAt = occurredAt;
    }

    /// <inheritdoc/>
    [Id(0)]
    public Guid InitiatedBy { get; init; }

    /// <inheritdoc/>
    [Id(1)]
    public DateTimeOffset OccurredAt { get; init; }
}
