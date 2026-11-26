using Continuum.CSharpFunctionalExtensions;

using Orleans.Concurrency;

namespace GetRDone.Orders.Abstractions.ReadModels;

/// <summary>
///     The staff order page of one submitted order (the OrderDetails read model): its summary, who it is assigned to, its
///     open document requests, its documents, its description of work, its timeline and what the signed-in user can do
///     next. Each order has one instance, keyed by the order ID, and only AMC staff (<see cref="StaffRoles.Staff"/>) can
///     read it.
/// </summary>
public interface IOrderDetailsGrain : IGrainWithStringKey
{
    /// <summary>Gets the order's details; fails with not found until the order is submitted.</summary>
    [ReadOnly]
    Task<Result<OrderDetails>> GetOrderDetailsAsync();
}

/// <summary>What the staff order page shows of an order.</summary>
/// <param name="OrderId">The order.</param>
/// <param name="OrderNumber">The order number, or null until the order number assigner gives the order its number.</param>
/// <param name="Status">How far the order is.</param>
/// <param name="ClientName">The client's name, or null when the user directory doesn't know the client.</param>
/// <param name="EnteredByUserId">The client user who submitted the order, as a tenant user ID.</param>
/// <param name="EnteredByName">Their name, or null when the user directory doesn't know them.</param>
/// <param name="FormTypeName">The form type ordered.</param>
/// <param name="PropertyAddress">The property address on one line.</param>
/// <param name="DueDate">When the client gets the report, or null until the order is assigned to a vendor.</param>
/// <param name="ServiceFee">What the client pays the AMC, or null until the order is assigned to a vendor.</param>
/// <param name="AccessContactName">Who gives access to the property, if the client named someone.</param>
/// <param name="AccessContactPhone">Their phone number, digits only.</param>
/// <param name="AssignedToUserId">The staff member the order is assigned to, or null until it is assigned.</param>
/// <param name="AssignedToName">Their name, or null when unassigned or the user directory doesn't know them.</param>
/// <param name="OpenRequests">The documents requested from the client that it has not provided yet, oldest first.</param>
/// <param name="Documents">The order's supporting documents, in the order they were added.</param>
/// <param name="DescriptionOfWork">The latest version of the order's description of work, or null until one is submitted.</param>
/// <param name="Vendor">The vendor the order is assigned to, or null until it is assigned to one.</param>
/// <param name="Timeline">What happened to the order, newest first.</param>
/// <param name="AllowedActions">What the signed-in user can do next.</param>
[GenerateSerializer, Immutable]
public sealed record OrderDetails(
    string OrderId,
    long? OrderNumber,
    StaffOrderStatus Status,
    string? ClientName,
    string EnteredByUserId,
    string? EnteredByName,
    string FormTypeName,
    string PropertyAddress,
    DateOnly? DueDate,
    decimal? ServiceFee,
    string? AccessContactName,
    string? AccessContactPhone,
    string? AssignedToUserId,
    string? AssignedToName,
    OrderDocumentRequestSummary[] OpenRequests,
    OrderDocumentSummary[] Documents,
    DescriptionOfWorkSummary? DescriptionOfWork,
    OrderVendorSummary? Vendor,
    OrderTimelineEntry[] Timeline,
    OrderActions AllowedActions);

/// <summary>The vendor an order is assigned to, as the staff order page shows it.</summary>
/// <param name="VendorId">The vendor, as a tenant-qualified vendor ID.</param>
/// <param name="VendorName">Its name, or null when the tenant's assignable vendors don't list it.</param>
/// <param name="VendorFee">What the AMC pays the vendor for the report.</param>
/// <param name="VendorDueDate">When the AMC needs the vendor's report.</param>
[GenerateSerializer, Immutable]
public sealed record OrderVendorSummary(
    string VendorId,
    string? VendorName,
    decimal VendorFee,
    DateOnly VendorDueDate);

/// <summary>The latest version of an order's description of work, as the staff and client order pages show it.</summary>
/// <param name="DescriptionOfWorkId">The description of work.</param>
/// <param name="Version">Its version; a revision is one more than the version it revises.</param>
/// <param name="Html">Its sanitized HTML; pages sanitize it again before they show it.</param>
/// <param name="Approved">Whether the client approved this version.</param>
/// <param name="SubmittedAt">When this version was submitted.</param>
[GenerateSerializer, Immutable]
public sealed record DescriptionOfWorkSummary(
    string DescriptionOfWorkId,
    int Version,
    string Html,
    bool Approved,
    DateTimeOffset SubmittedAt);

/// <summary>A supporting document of the order, which staff can download.</summary>
/// <param name="DocumentId">The document.</param>
/// <param name="FileName">The name of its file.</param>
/// <param name="DocumentTypeName">What kind of document it is.</param>
/// <param name="AddedByName">Who attached or provided it, or null when the user directory doesn't know them.</param>
/// <param name="AddedAt">When it was added.</param>
[GenerateSerializer, Immutable]
public sealed record OrderDocumentSummary(
    string DocumentId,
    string FileName,
    string DocumentTypeName,
    string? AddedByName,
    DateTimeOffset AddedAt);

/// <summary>A request for a document from the client that it has not provided yet.</summary>
/// <param name="RequestId">The request.</param>
/// <param name="DocumentTypeName">The type of document requested, such as <c>Rent roll</c>.</param>
/// <param name="Message">What the AMC told the client about it.</param>
/// <param name="RequestedAt">When it was requested.</param>
[GenerateSerializer, Immutable]
public sealed record OrderDocumentRequestSummary(
    string RequestId,
    string DocumentTypeName,
    string Message,
    DateTimeOffset RequestedAt);

/// <summary>An entry on an order's timeline, such as <c>Assigned to Scott Riggsbee</c>.</summary>
/// <param name="OccurredAt">When it happened.</param>
/// <param name="Entry">What happened.</param>
[GenerateSerializer, Immutable]
public sealed record OrderTimelineEntry(DateTimeOffset OccurredAt, string Entry);

/// <summary>What the signed-in user can do next on the staff order page.</summary>
[Flags]
public enum OrderActions
{
    None = 0,

    /// <summary>Assign the submitted order to themselves; for the Client Services Director or the tenant admin.</summary>
    AssignToMe = 1,

    /// <summary>Confirm they are working on the order; for the staff member it is assigned to.</summary>
    Acknowledge = 2,

    /// <summary>
    ///     Ask the client for a document once the order is acknowledged; for the staff member it is assigned to, the Client
    ///     Services Director or the tenant admin.
    /// </summary>
    RequestDocument = 4,

    /// <summary>
    ///     Submit the description of work for the client's approval once the order is acknowledged, or revise it until the
    ///     client approves it; for the staff member it is assigned to, the Client Services Director or the tenant admin.
    /// </summary>
    SubmitDescriptionOfWork = 8,

    /// <summary>
    ///     Assign the order to one of the tenant's vendors once the client approves the description of work; for the staff
    ///     member it is assigned to, the Client Services Director or the tenant admin.
    /// </summary>
    AssignVendor = 16
}
