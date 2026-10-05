using Continuum.TypeMapping;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.Events;

[DomainEventType("Tests.SnackRemovedEvent"), Immutable, GenerateSerializer]
public sealed record SnackRemovedEvent(Guid Id, Guid TraceId, DateTimeOffset OperatedAt, string OperatedBy, int Version) 
    : SnackEvent(Id, TraceId, OperatedAt, OperatedBy, Version);
