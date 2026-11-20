namespace Continuum.Domain;

/// <summary>A domain event that records who initiated it and when it occurred.</summary>
public interface IDomainEvent
{
    /// <summary>Gets the identifier of the user or system user whose identity initiated the action.</summary>
    Guid InitiatedBy { get; }

    /// <summary>Gets the time at which the event occurred.</summary>
    DateTimeOffset OccurredAt { get; }
}
