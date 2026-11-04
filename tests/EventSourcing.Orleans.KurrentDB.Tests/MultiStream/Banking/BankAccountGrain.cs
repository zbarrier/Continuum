using Continuum.Domain.Orleans;

using Orleans.Providers;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.MultiStream.Banking;

public partial interface IBankAccountGrain : IEventDrivenGrain
{

}


[LogConsistencyProvider(ProviderName = ProviderConstants.DEFAULT_LOG_CONSISTENCY_PROVIDER_NAME)]
[StorageProvider(ProviderName = "MemoryStorageProvider")]
public partial class BankAccountGrain : EventDrivenGrain<BankAccountState, BankAccountEvent>, IBankAccountGrain
{
    /// <inheritdoc />
    public BankAccountGrain(IServiceProvider serviceProvider)
        : base(serviceProvider, "journaledGrainLog")
    {
        // When another account transfers money to this one, this account takes part in that transfer by crediting itself.
        OnTransaction<CreditAccountCommand>(CreditAsyncCore);
    }
}
