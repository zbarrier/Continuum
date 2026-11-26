using Continuum.CSharpFunctionalExtensions;

using Orleans.Concurrency;

namespace GetRDone.Orders.Abstractions.ReadModels;

/// <summary>
///     The orders the tenant assigned to vendors (the VendorOrders read model), due soonest first. Each tenant has one
///     instance, keyed <c>{tenantId}|default</c>, and a vendor user sees only the orders of their own vendor.
/// </summary>
public interface IVendorOrdersGrain : IGrainWithStringKey
{
    /// <summary>
    ///     Gets up to <paramref name="take"/> of the signed-in vendor user's vendor's orders after skipping
    ///     <paramref name="skip"/>, with the total count.
    /// </summary>
    [ReadOnly]
    Task<Result<VendorOrdersPage>> GetOrdersAsync(int skip, int take);
}

/// <summary>An order's status as its vendor sees it.</summary>
public enum VendorOrderStatus
{
    /// <summary>The AMC assigned the order to the vendor.</summary>
    Assigned,

    /// <summary>The vendor scheduled the property's inspection.</summary>
    InspectionScheduled,

    /// <summary>The vendor's appraiser inspected the property.</summary>
    InspectionCompleted,

    /// <summary>The vendor delivered its completed report.</summary>
    ReportDelivered
}

/// <summary>An order on the vendor's Orders list.</summary>
/// <param name="OrderId">The order.</param>
/// <param name="OrderNumber">The order number, or null when the order was assigned before it was given its number.</param>
/// <param name="PropertyAddress">The property address on one line.</param>
/// <param name="FormTypeName">The form type ordered.</param>
/// <param name="Status">The order's status as the vendor sees it.</param>
/// <param name="VendorDueDate">When the AMC needs the vendor's report.</param>
[GenerateSerializer, Immutable]
public sealed record VendorOrder(
    string OrderId,
    long? OrderNumber,
    string PropertyAddress,
    string FormTypeName,
    VendorOrderStatus Status,
    DateOnly VendorDueDate);

/// <summary>A page of <see cref="VendorOrder"/>s.</summary>
/// <param name="Orders">The orders on the page.</param>
/// <param name="TotalCount">The number of orders on all pages.</param>
[GenerateSerializer, Immutable]
public sealed record VendorOrdersPage(
    VendorOrder[] Orders,
    int TotalCount);
