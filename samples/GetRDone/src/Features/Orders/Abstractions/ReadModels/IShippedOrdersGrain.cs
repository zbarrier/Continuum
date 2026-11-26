using Continuum.CSharpFunctionalExtensions;

using Orleans.Concurrency;

namespace GetRDone.Orders.Abstractions.ReadModels;

/// <summary>
///     The Shipped list on the staff dashboard (the ShippedOrders read model): every order of the tenant the AMC shipped, newest
///     first. Each tenant has one instance, keyed <c>{tenantId}|default</c>, and only AMC staff (<see cref="StaffRoles.Staff"/>)
///     can read it.
/// </summary>
public interface IShippedOrdersGrain : IGrainWithStringKey
{
    /// <summary>Gets up to <paramref name="take"/> orders after skipping <paramref name="skip"/>, with the total count.</summary>
    [ReadOnly]
    Task<Result<ShippedOrdersPage>> GetOrdersAsync(int skip, int take);
}

/// <summary>An order on the Shipped list.</summary>
/// <param name="OrderId">The order.</param>
/// <param name="OrderNumber">The order number, or null when the order was shipped before it was given its number.</param>
/// <param name="ClientName">The client's name, or null when the user directory doesn't know the client.</param>
/// <param name="PropertyAddress">The property address on one line.</param>
/// <param name="ShippedAt">When the AMC shipped the report.</param>
[GenerateSerializer, Immutable]
public sealed record ShippedOrder(
    string OrderId,
    long? OrderNumber,
    string? ClientName,
    string PropertyAddress,
    DateTimeOffset ShippedAt);

/// <summary>A page of <see cref="ShippedOrder"/>s.</summary>
/// <param name="Orders">The orders on the page.</param>
/// <param name="TotalCount">The number of orders on all pages.</param>
[GenerateSerializer, Immutable]
public sealed record ShippedOrdersPage(
    ShippedOrder[] Orders,
    int TotalCount);
