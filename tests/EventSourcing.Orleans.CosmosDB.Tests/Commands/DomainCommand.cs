namespace Continuum.EventSourcing.Orleans.CosmosDB.Tests.Commands;

[Immutable]
[GenerateSerializer]
public abstract record DomainCommand(Guid TraceId, DateTimeOffset OperatedAt, string OperatedBy);
