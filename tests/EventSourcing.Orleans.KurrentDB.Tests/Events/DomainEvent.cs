namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.Events;

[Immutable, GenerateSerializer]
public abstract record DomainEvent(Guid TraceId, DateTimeOffset OperatedAt, string OperatedBy, int Version);
