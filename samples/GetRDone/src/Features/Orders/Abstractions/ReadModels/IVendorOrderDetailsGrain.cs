using Continuum.CSharpFunctionalExtensions;

using Orleans.Concurrency;

namespace GetRDone.Orders.Abstractions.ReadModels;

/// <summary>
///     The vendor order page of one order assigned to a vendor (the VendorOrderDetails read model): the engagement, the
///     property, its access contact, the approved description of work, the inspection, the report, the timeline and what
///     the vendor's users can do next. Each order has one instance, keyed by the order ID, and only users of the vendor the
///     order is assigned to can read it.
/// </summary>
public interface IVendorOrderDetailsGrain : IGrainWithStringKey
{
    /// <summary>Gets the order's details; fails with not found until the order is assigned to a vendor.</summary>
    [ReadOnly]
    Task<Result<VendorOrderDetails>> GetVendorOrderDetailsAsync();
}

/// <summary>What the vendor order page shows of an order.</summary>
/// <param name="OrderId">The order.</param>
/// <param name="OrderNumber">The order number, or null until the order number assigner gives the order its number.</param>
/// <param name="Status">The order's status as the vendor sees it.</param>
/// <param name="ClientName">The client's name, or null when the user directory doesn't know the client.</param>
/// <param name="FormTypeName">The form type ordered.</param>
/// <param name="PropertyAddress">The property address on one line.</param>
/// <param name="AccessContactName">Who gives access to the property, if the client named someone.</param>
/// <param name="AccessContactPhone">Their phone number, digits only.</param>
/// <param name="VendorFee">What the AMC pays the vendor for the report.</param>
/// <param name="VendorDueDate">When the AMC needs the vendor's report.</param>
/// <param name="DescriptionOfWork">The description of work the client approved.</param>
/// <param name="Inspection">The property's inspection, or null until the vendor schedules it.</param>
/// <param name="Report">The vendor's completed report, or null until the vendor delivers it.</param>
/// <param name="Timeline">What happened to the order since it was assigned to the vendor, newest first.</param>
/// <param name="AllowedActions">What the vendor's users can do next.</param>
[GenerateSerializer, Immutable]
public sealed record VendorOrderDetails(
    string OrderId,
    long? OrderNumber,
    VendorOrderStatus Status,
    string? ClientName,
    string FormTypeName,
    string PropertyAddress,
    string? AccessContactName,
    string? AccessContactPhone,
    decimal VendorFee,
    DateOnly VendorDueDate,
    DescriptionOfWorkSummary DescriptionOfWork,
    OrderInspectionSummary? Inspection,
    OrderReportSummary? Report,
    OrderTimelineEntry[] Timeline,
    VendorOrderActions AllowedActions);

/// <summary>What the vendor's users can do next on the vendor order page; each step is done once, in this order.</summary>
[Flags]
public enum VendorOrderActions
{
    None = 0,

    /// <summary>Schedule the property's inspection once the order is assigned to the vendor.</summary>
    ScheduleInspection = 1,

    /// <summary>Record that the appraiser inspected the property once the inspection is scheduled.</summary>
    CompleteInspection = 2,

    /// <summary>Upload the completed report once the inspection is completed.</summary>
    DeliverReport = 4
}
