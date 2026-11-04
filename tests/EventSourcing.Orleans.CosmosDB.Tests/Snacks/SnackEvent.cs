namespace Continuum.EventSourcing.Orleans.CosmosDB.Tests.Snacks;

[Immutable, GenerateSerializer]
public abstract record SnackEvent(Guid Id, DateTimeOffset OccurredAt);