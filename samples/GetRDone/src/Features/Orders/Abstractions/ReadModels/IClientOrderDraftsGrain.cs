using Continuum.CSharpFunctionalExtensions;

using Orleans.Concurrency;

namespace GetRDone.Orders.Abstractions.ReadModels;

/// <summary>
///     The drafts a client user started and has not submitted yet (the ClientOrderDrafts read model), most recently saved
///     first. Each user has their own instance, keyed by their tenant user ID <c>{TenantId}|{UserGuid}</c>, and only that
///     user can read it.
/// </summary>
public interface IClientOrderDraftsGrain : IGrainWithStringKey
{
    /// <summary>Gets up to <paramref name="take"/> drafts after skipping <paramref name="skip"/>, with the total count.</summary>
    [ReadOnly]
    Task<Result<ClientOrderDraftsPage>> GetDraftsAsync(int skip, int take);
}

/// <summary>A draft on the client user's My drafts list.</summary>
/// <param name="OrderId">The order the draft belongs to.</param>
/// <param name="PropertyAddress">The property address on one line.</param>
/// <param name="FormTypeName">The form type ordered.</param>
/// <param name="LastSavedAt">When the draft was last saved.</param>
[GenerateSerializer, Immutable]
public sealed record ClientOrderDraft(
    string OrderId,
    string PropertyAddress,
    string FormTypeName,
    DateTimeOffset LastSavedAt);

/// <summary>A page of <see cref="ClientOrderDraft"/>s.</summary>
/// <param name="Drafts">The drafts on the page.</param>
/// <param name="TotalCount">The number of drafts on all pages.</param>
[GenerateSerializer, Immutable]
public sealed record ClientOrderDraftsPage(
    ClientOrderDraft[] Drafts,
    int TotalCount);
