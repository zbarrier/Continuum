using Continuum.Orleans.Events;
using Continuum.TypeMapping;

using Orleans.EventSourcing;
using Orleans.Providers;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests.SingleStream.Tallies;

public interface ITallyGrain : IGrainWithStringKey
{
    Task<TallyView> GetAsync();

    Task<int> AddAsync(int value);

    Task DeactivateAsync();
}

/// <summary>
///     A grain that loads only its 3 most recent events. Its state keeps every value applied, without trimming, so tests
///     can see exactly which events were loaded.
/// </summary>
[GrainType(GrainTypeName)]
[LogConsistencyProvider(ProviderName = ProviderConstants.DEFAULT_LOG_CONSISTENCY_PROVIDER_NAME)]
[StorageProvider(ProviderName = "MemoryStorageProvider")]
[LoadMostRecentEventsOnly(MostRecentEventCount)]
public sealed class TallyGrain : JournaledGrain<TallyState, TallyAdded>, ITallyGrain
{
    public const string GrainTypeName = "tally";
    public const int MostRecentEventCount = 3;

    public Task<TallyView> GetAsync() => Task.FromResult(new TallyView(Version, [.. State.Values]));

    public async Task<int> AddAsync(int value)
    {
        RaiseEvent(new TallyAdded(value));
        await ConfirmEvents();
        return Version;
    }

    public Task DeactivateAsync()
    {
        this.DeactivateOnIdle();
        return Task.CompletedTask;
    }

    protected override void TransitionState(TallyState state, TallyAdded @event) => state.Values.Add(@event.Value);
}

[GenerateSerializer]
public sealed class TallyState
{
    [Id(0)]
    public List<int> Values { get; set; } = [];
}

[DomainEventType("Tests.TallyAdded"), GenerateSerializer, Immutable]
public sealed record TallyAdded(int Value);

[GenerateSerializer, Immutable]
public sealed record TallyView(int Version, int[] Values);
