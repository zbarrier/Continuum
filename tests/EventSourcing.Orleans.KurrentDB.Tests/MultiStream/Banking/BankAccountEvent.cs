namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.MultiStream.Banking;

[Immutable, GenerateSerializer]
public abstract record BankAccountEvent(Guid AccountId, DateTimeOffset OccurredAt);
