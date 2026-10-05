namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.Commands;

[Immutable]
[GenerateSerializer]
public abstract record DomainCommand(Guid TraceId, DateTimeOffset OperatedAt, string OperatedBy);
