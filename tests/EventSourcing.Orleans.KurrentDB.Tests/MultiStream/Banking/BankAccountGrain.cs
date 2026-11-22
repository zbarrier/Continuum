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

    /// <summary>
    ///     Sends each event to this account's activity stream as well, and a credit to the paying account's stream too.
    ///     A credit made during a transfer is this account's event, so this account chooses its targets, even though the
    ///     paying account coordinates the transfer.
    /// </summary>
    protected override IReadOnlyList<GrainId> GetStreamTargets(object @event)
    {
        var activity = ActivityStreamTarget(this.GetPrimaryKeyString());
        return @event is MoneyCreditedEvent credit
            ? [activity, GrainFactory.GetGrain<IBankAccountGrain>(credit.FromAccountId.ToString()).GetGrainId()]
            : [activity];
    }

    /// <summary>Gets the stream target that receives every event of an account.</summary>
    public static GrainId ActivityStreamTarget(string accountId) => GrainId.Create("accountactivity", accountId);
}
