namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.Commands;

[Immutable]
[GenerateSerializer]
public sealed record SnackRepoDeleteCommand(Guid Id, Guid TraceId, DateTimeOffset OperatedAt, string OperatedBy) 
    : DomainCommand(TraceId, OperatedAt, OperatedBy);
