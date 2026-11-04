using Continuum.Domain.Orleans;

using Orleans.Providers;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.SingleStream.Snacks;

public partial interface ISnackGrain : IEventDrivenGrain
{

}


[LogConsistencyProvider(ProviderName = ProviderConstants.DEFAULT_LOG_CONSISTENCY_PROVIDER_NAME)]
[StorageProvider(ProviderName = "MemoryStorageProvider")]
public partial class SnackGrain : EventDrivenGrain<SnackState, SnackEvent>, ISnackGrain
{
    /// <inheritdoc />
    public SnackGrain(IServiceProvider serviceProvider)
        : base(serviceProvider, "journaledGrainLog")
    {
    }
}
