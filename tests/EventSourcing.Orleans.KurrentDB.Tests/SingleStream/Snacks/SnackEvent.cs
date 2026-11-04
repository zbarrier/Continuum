namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.SingleStream.Snacks;

[Immutable, GenerateSerializer]
public abstract record SnackEvent(Guid Id, DateTimeOffset OccurredAt);