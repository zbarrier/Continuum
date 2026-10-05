using System.Collections.Immutable;

using Continuum.TypeMapping;

namespace Continuum.EventSourcing.Orleans.CosmosDB.Tests.Events;

[DomainEventType("Tests.ErrorOccurredEvent"), Immutable, GenerateSerializer]
public abstract record ErrorOccurredEvent(int Code, IImmutableList<string> Reasons, Guid TraceId, DateTimeOffset OperatedAt, string OperatedBy, int Version) 
    : DomainEvent(TraceId, OperatedAt, OperatedBy, Version);
