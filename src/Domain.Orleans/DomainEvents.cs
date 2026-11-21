namespace Continuum.Domain.Orleans;

/// <summary>Base class for domain events that record who initiated them and when they occurred.</summary>
/// <remarks>
///     Marked <see cref="ImmutableAttribute"/>: domain events must always be immutable, so derived events must not expose mutable state.
///     Properties are <see langword="required"/> <see langword="init"/> so derived events need no constructor and are created with an object initializer.
/// </remarks>
[GenerateSerializer, Immutable]
public abstract class DomainEvent : IDomainEvent
{
    /// <inheritdoc/>
    [Id(0)]
    public required string InitiatedBy { get; init; }

    /// <inheritdoc/>
    [Id(1)]
    public required DateTimeOffset OccurredAt { get; init; }
}
