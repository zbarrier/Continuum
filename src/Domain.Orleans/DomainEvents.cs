namespace Continuum.Domain.Orleans;

/// <summary>Base class for domain events that record who performed them and when.</summary>
/// <remarks>Marked <see cref="ImmutableAttribute"/>: domain events must always be immutable, so derived events must not expose mutable state.</remarks>
[GenerateSerializer, Immutable]
public abstract class DomainEvent : IDomainEvent
{
    /// <summary>Initializes a new instance of the <see cref="DomainEvent"/> class.</summary>
    /// <param name="performedBy">The identifier of the user or actor that performed the action.</param>
    /// <param name="performedOn">The time at which the action was performed.</param>
    public DomainEvent(Guid performedBy, DateTime performedOn)
    {
        PerformedBy = performedBy;
        PerformedOn = performedOn;
    }

    /// <inheritdoc/>
    [Id(0)]
    public Guid PerformedBy { get; init; }

    /// <inheritdoc/>
    [Id(1)]
    public DateTime PerformedOn { get; init; }
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
    /// <param name="performedBy">The identifier of the user or actor that performed the action.</param>
    /// <param name="performedOn">The time at which the action was performed.</param>
    public StateMachineDomainEvent(TCommand command, Guid performedBy, DateTime performedOn)
        : base(command)
    {
        PerformedBy = performedBy;
        PerformedOn = performedOn;
    }

    /// <inheritdoc/>
    [Id(0)]
    public Guid PerformedBy { get; init; }

    /// <inheritdoc/>
    [Id(1)]
    public DateTime PerformedOn { get; init; }
}
