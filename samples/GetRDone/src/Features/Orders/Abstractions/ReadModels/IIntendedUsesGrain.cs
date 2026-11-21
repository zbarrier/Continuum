using Continuum.CSharpFunctionalExtensions;

using Orleans.Concurrency;

namespace GetRDone.Orders.Abstractions.ReadModels;

/// <summary>
///     The intended uses a client user can choose when ordering. The sample seeds them per tenant instead of projecting
///     them from intended use events.
/// </summary>
public interface IIntendedUsesGrain : IGrainWithStringKey
{
    [ReadOnly]
    Task<IntendedUse[]> GetIntendedUsesAsync();

    /// <summary>Succeeds when the intended use exists, the version is its current version, and the name matches.</summary>
    [ReadOnly]
    Task<Result> IsCurrentIntendedUseAsync(IntendedUseRef intendedUse);
}

[GenerateSerializer, Immutable]
public sealed record IntendedUse(
    string IntendedUseId,
    int IntendedUseVersion,
    string Name);
