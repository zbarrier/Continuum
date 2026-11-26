using Continuum.CSharpFunctionalExtensions;

using Orleans.Concurrency;

namespace GetRDone.Orders.Abstractions.ReadModels;

/// <summary>
///     The vendors the AMC can assign orders to (the event model's AssignableVendors). The sample seeds them per tenant
///     instead of projecting them from vendor events.
/// </summary>
public interface IAssignableVendorsGrain : IGrainWithStringKey
{
    [ReadOnly]
    Task<AssignableVendor[]> GetVendorsAsync();

    /// <summary>Succeeds when the vendor is one the AMC can assign orders to.</summary>
    [ReadOnly]
    Task<Result> IsAssignableVendorAsync(VendorId vendorId);
}

/// <summary>A vendor the AMC can assign orders to, with the states its appraisers are licensed in.</summary>
[GenerateSerializer, Immutable]
public sealed record AssignableVendor(
    string VendorId,
    string Name,
    string[] LicensedStates);
