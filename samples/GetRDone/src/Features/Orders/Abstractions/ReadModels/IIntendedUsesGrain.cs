using Orleans.Concurrency;

namespace GetRDone.Orders.Abstractions.ReadModels;

public interface IIntendedUsesGrain : IGrainWithStringKey
{
    [ReadOnly]
    Task<IntendedUse[]> GetIntendedUsesAsync();
}

[GenerateSerializer, Immutable]
public sealed record IntendedUse(
    Guid IntendedUseId,
    int IntendedUseVersion,
    string Name);
