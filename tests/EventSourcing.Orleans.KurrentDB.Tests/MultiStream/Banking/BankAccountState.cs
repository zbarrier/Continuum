using Continuum.Domain.Orleans;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.MultiStream.Banking;

[GenerateSerializer]
public sealed partial class BankAccountState : EventDrivenState<BankAccountState, BankAccountEvent>
{
    public BankAccountState()
    {
        On<BankAccountOpenedEvent>(Handle);
        On<BankAccountClosedEvent>(Handle);
        On<MoneyDebitedEvent>(Handle);
        On<MoneyCreditedEvent>(Handle);
    }

    [Id(0)]
    public Guid Id { get; set; }

    [Id(1)]
    public bool IsOpen { get; set; }

    [Id(2)]
    public decimal Balance { get; set; }

    [Id(3)]
    public decimal OverdraftLimit { get; set; }

    public decimal AvailableFunds => Balance + OverdraftLimit;
}
