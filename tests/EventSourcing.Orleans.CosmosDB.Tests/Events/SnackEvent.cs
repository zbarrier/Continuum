namespace Continuum.EventSourcing.Orleans.CosmosDB.Tests.Events;

[Immutable, GenerateSerializer]
public abstract record SnackEvent(Guid Id, Guid TraceId, DateTimeOffset OperatedAt, string OperatedBy, int Version) 
    : DomainEvent(TraceId, OperatedAt, OperatedBy, Version);
