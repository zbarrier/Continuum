using Continuum.CSharpFunctionalExtensions;

using Orleans.Concurrency;

namespace GetRDone.Orders.Abstractions.ReadModels;

/// <summary>
///     The Unassigned list on the staff dashboard (the UnassignedOrders read model): the tenant's submitted orders that have
///     their number and nobody assigned yet, oldest order number first. Each tenant has one instance, keyed <c>{tenantId}|default</c>,
///     and only AMC staff (<see cref="StaffRoles.Staff"/>) can read it.
/// </summary>
public interface IUnassignedOrdersGrain : IGrainWithStringKey
{
    /// <summary>Gets up to <paramref name="take"/> orders after skipping <paramref name="skip"/>, with the total count.</summary>
    [ReadOnly]
    Task<Result<UnassignedOrdersPage>> GetOrdersAsync(int skip, int take);
}

/// <summary>An order on the Unassigned list.</summary>
/// <param name="OrderId">The order.</param>
/// <param name="OrderNumber">The order number.</param>
/// <param name="ClientName">The client's name, or null when the user directory doesn't know the client.</param>
/// <param name="PropertyAddress">The property address on one line.</param>
/// <param name="FormTypeName">The form type ordered.</param>
[GenerateSerializer, Immutable]
public sealed record UnassignedOrder(
    string OrderId,
    long OrderNumber,
    string? ClientName,
    string PropertyAddress,
    string FormTypeName);

/// <summary>A page of <see cref="UnassignedOrder"/>s.</summary>
/// <param name="Orders">The orders on the page.</param>
/// <param name="TotalCount">The number of orders on all pages.</param>
[GenerateSerializer, Immutable]
public sealed record UnassignedOrdersPage(
    UnassignedOrder[] Orders,
    int TotalCount);
