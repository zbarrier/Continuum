using Continuum.CSharpFunctionalExtensions;

using Orleans.Concurrency;

namespace GetRDone.Orders.Abstractions.ReadModels;

/// <summary>
///     The orders a client user submitted (the ClientOrders read model), with their number and status, most recently
///     submitted first. Each user has their own instance, keyed by their tenant user ID <c>{TenantId}|{UserGuid}</c>, and
///     only that user can read it.
/// </summary>
public interface IClientOrdersGrain : IGrainWithStringKey
{
    /// <summary>Gets up to <paramref name="take"/> orders after skipping <paramref name="skip"/>, with the total count.</summary>
    [ReadOnly]
    Task<Result<ClientOrdersPage>> GetOrdersAsync(int skip, int take);
}

/// <summary>An order's status as the client sees it.</summary>
public enum ClientOrderStatus
{
    /// <summary>The order is submitted to the AMC.</summary>
    Submitted,

    /// <summary>The AMC assigned the order to a staff member.</summary>
    Assigned,

    /// <summary>The staff member the order is assigned to is working on it.</summary>
    Acknowledged,

    /// <summary>The order's description of work waits for the client's approval.</summary>
    DescriptionOfWorkAwaitingApproval,

    /// <summary>The client approved the order's description of work.</summary>
    DescriptionOfWorkApproved,

    /// <summary>The AMC assigned the order to an appraiser.</summary>
    VendorAssigned,

    /// <summary>The appraiser scheduled the property's inspection.</summary>
    InspectionScheduled,

    /// <summary>
    ///     The appraiser inspected the property. The order stays here while the AMC reviews the appraiser's report, until it
    ///     ships the report to the client.
    /// </summary>
    InspectionCompleted
}

/// <summary>A submitted order on the client user's My orders list.</summary>
/// <param name="OrderId">The order.</param>
/// <param name="OrderNumber">The order number, or null until the order number assigner gives the order its number.</param>
/// <param name="PropertyAddress">The property address on one line.</param>
/// <param name="FormTypeName">The form type ordered.</param>
/// <param name="Status">The order's status as the client sees it.</param>
/// <param name="SubmittedAt">When the order was submitted.</param>
[GenerateSerializer, Immutable]
public sealed record ClientOrder(
    string OrderId,
    long? OrderNumber,
    string PropertyAddress,
    string FormTypeName,
    ClientOrderStatus Status,
    DateTimeOffset SubmittedAt);

/// <summary>A page of <see cref="ClientOrder"/>s.</summary>
/// <param name="Orders">The orders on the page.</param>
/// <param name="TotalCount">The number of orders on all pages.</param>
[GenerateSerializer, Immutable]
public sealed record ClientOrdersPage(
    ClientOrder[] Orders,
    int TotalCount);
