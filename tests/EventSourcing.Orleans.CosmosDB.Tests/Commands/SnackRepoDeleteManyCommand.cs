namespace Continuum.EventSourcing.Orleans.CosmosDB.Tests.Commands;

[Immutable]
[GenerateSerializer]
public sealed record SnackRepoDeleteManyCommand(Guid[] Ids, Guid TraceId, DateTimeOffset OperatedAt, string OperatedBy) 
    : DomainCommand(TraceId, OperatedAt, OperatedBy);
