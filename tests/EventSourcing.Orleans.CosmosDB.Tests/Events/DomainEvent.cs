namespace Continuum.EventSourcing.Orleans.CosmosDB.Tests.Events;

[Immutable, GenerateSerializer]
public abstract record DomainEvent(Guid TraceId, DateTimeOffset OperatedAt, string OperatedBy, int Version);
