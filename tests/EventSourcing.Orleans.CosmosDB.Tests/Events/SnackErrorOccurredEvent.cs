using System.Collections.Immutable;

using Continuum.TypeMapping;

namespace Continuum.EventSourcing.Orleans.CosmosDB.Tests.Events;

[DomainEventType("Tests.SnackErrorOccurredEvent"), Immutable, GenerateSerializer]
public sealed record SnackErrorOccurredEvent(Guid Id, int Code, IImmutableList<string> Reasons, Guid TraceId, DateTimeOffset OperatedAt, string OperatedBy, int Version) 
    : ErrorOccurredEvent(Code, Reasons, TraceId, OperatedAt, OperatedBy, Version);
