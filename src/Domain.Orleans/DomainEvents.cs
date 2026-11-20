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
