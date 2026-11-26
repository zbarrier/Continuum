using Continuum.CSharpFunctionalExtensions;

using Orleans.Concurrency;

namespace GetRDone.Orders.Abstractions.ReadModels;

/// <summary>
///     The In progress list on the staff dashboard (the InProgressOrders read model): every assigned order of the tenant that
///     is not shipped yet, with
///     who it is assigned to and its status, due soonest first, then the orders without a due date yet by order number.
///     Each tenant has one instance, keyed <c>{tenantId}|default</c>,
///     and only AMC staff (<see cref="StaffRoles.Staff"/>) can read it.
/// </summary>
public interface IInProgressOrdersGrain : IGrainWithStringKey
{
    /// <summary>Gets up to <paramref name="take"/> orders after skipping <paramref name="skip"/>, with the total count.</summary>
    [ReadOnly]
    Task<Result<InProgressOrdersPage>> GetOrdersAsync(int skip, int take);
}

/// <summary>An order on the In progress list.</summary>
/// <param name="OrderId">The order.</param>
/// <param name="OrderNumber">The order number, or null when the order was assigned before it was given its number.</param>
/// <param name="ClientName">The client's name, or null when the user directory doesn't know the client.</param>
/// <param name="PropertyAddress">The property address on one line.</param>
/// <param name="AssignedToUserId">The staff member the order is assigned to, as a tenant user ID.</param>
/// <param name="AssignedToName">Their name, or null when the user directory doesn't know them.</param>
/// <param name="Status">How far the order is.</param>
/// <param name="DueDate">When the client gets the report, or null until the order is assigned to a vendor.</param>
[GenerateSerializer, Immutable]
public sealed record InProgressOrder(
    string OrderId,
    long? OrderNumber,
    string? ClientName,
    string PropertyAddress,
    string AssignedToUserId,
    string? AssignedToName,
    StaffOrderStatus Status,
    DateOnly? DueDate);

/// <summary>A page of <see cref="InProgressOrder"/>s.</summary>
/// <param name="Orders">The orders on the page.</param>
/// <param name="TotalCount">The number of orders on all pages.</param>
[GenerateSerializer, Immutable]
public sealed record InProgressOrdersPage(
    InProgressOrder[] Orders,
    int TotalCount);
