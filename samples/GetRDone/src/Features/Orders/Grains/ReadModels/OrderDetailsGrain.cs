using Continuum.CSharpFunctionalExtensions;
using Continuum.CSharpFunctionalExtensions.Orleans;
using Continuum.Orleans.Tenancy;
using Continuum.Streaming;
using Continuum.Streaming.Orleans;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Abstractions.ReadModels;

using ManagedCode.Orleans.Identity.Core.Extensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace GetRDone.Orders.Grains.ReadModels;

/// <summary>
///     Projects the staff order page of one order from the order's own stream (see <see cref="OrderProjections"/>).
/// </summary>
/// <remarks>
///     The stream provider activates the grain whose key is the stream key, the order ID, so there is one instance per
///     order. <see cref="AuthorizeAttribute"/> on the class covers the query and
///     <see cref="IProjectionGrain.OnNextBatchAsync"/>, which a catch-up subscription calls signed in as its automation
///     account; stream deliveries arrive through Orleans' stream consumer extension, which it does not cover.
/// </remarks>
[ImplicitStreamSubscription(OrderProjections.OrderEventsNamespace)]
[GrainType("orderDetails")]
[Authorize]
public sealed class OrderDetailsGrain(
    [PersistentState("orderDetails", OrderProjections.StorageName)] IPersistentState<OrderDetailsState> storage,
    ILogger<OrderDetailsGrain> logger)
    : ProjectionGrain<OrderDetailsGrain, OrderDetailsState>(storage, logger), IOrderDetailsGrain
{
    public async Task<Result<OrderDetails>> GetOrderDetailsAsync()
    {
        return await this.EnsureStaff()
            .Ensure(() => State.Submitted, () => RequestErrors.NewNotFound("The order was not found."))
            .BindTry(async () =>
            {
                TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);
                var names = await UserDirectoryNames.LoadAsync(GrainFactory, tenantId);

                return Result.Success(new OrderDetails(
                    this.GetPrimaryKeyString(),
                    State.OrderNumber,
                    State.Status,
                    names.ClientName(State.ClientId!),
                    State.EnteredBy!,
                    names.UserName(State.EnteredBy),
                    State.FormTypeName!,
                    State.PropertyAddress!,
                    State.DueDate,
                    State.ServiceFee,
                    State.AccessContactName,
                    State.AccessContactPhone,
                    State.AssignedTo,
                    names.UserName(State.AssignedTo),
                    State.OpenRequests.ToArray(),
                    State.Documents
                        .Select(document => new OrderDocumentSummary(
                            document.DocumentId, document.FileName, document.DocumentTypeName,
                            names.UserName(document.AddedBy), document.AddedAt))
                        .ToArray(),
                    State.DescriptionOfWork,
                    State.Vendor is { } vendor
                        ? new OrderVendorSummary(vendor.VendorId, names.VendorName(vendor.VendorId), vendor.VendorFee, vendor.VendorDueDate)
                        : null,
                    State.Inspection,
                    State.Report,
                    OrderTimeline.Describe(State.Timeline, names, OrderTimelineAudience.Staff),
                    GetAllowedActions(tenantId)));
            }, ex => GlobalGrainErrorHandler.Default(ex, logger));
    }

    private OrderActions GetAllowedActions(string? tenantId) => State.Status switch
    {
        StaffOrderStatus.Submitted when this.GetCurrentUser().IsInAnyRole(StaffRoles.OrderAssigners) => OrderActions.AssignToMe,
        StaffOrderStatus.Assigned when State.AssignedTo == this.GetCurrentTenantUserId(tenantId) => OrderActions.Acknowledge,
        StaffOrderStatus.Acknowledged or StaffOrderStatus.DescriptionOfWorkAwaitingApproval when IsAssigneeOrAssigner(tenantId)
            => OrderActions.RequestDocument | OrderActions.SubmitDescriptionOfWork,
        StaffOrderStatus.DescriptionOfWorkApproved when IsAssigneeOrAssigner(tenantId)
            => OrderActions.RequestDocument | OrderActions.AssignVendor,
        StaffOrderStatus.VendorAssigned or StaffOrderStatus.InspectionScheduled or StaffOrderStatus.InspectionCompleted
            or StaffOrderStatus.ReportDelivered when IsAssigneeOrAssigner(tenantId) => OrderActions.RequestDocument,
        _ => OrderActions.None
    };

    private bool IsAssigneeOrAssigner(string? tenantId)
        => State.AssignedTo == this.GetCurrentTenantUserId(tenantId) || this.GetCurrentUser().IsInAnyRole(StaffRoles.OrderAssigners);
}

[GenerateSerializer]
public sealed class OrderDetailsState : HandlerProjectionState<OrderDetailsState>
{
    public OrderDetailsState()
    {
        On<OrderDraftStarted>(Apply);
        On<RequestAndPropertySaved>(Apply);
        On<BorrowerAndAccessContactSaved>(Apply);
        On<SupportingDocumentAttached>(Apply);
        On<SupportingDocumentRemoved>(Apply);
        On<OrderSubmitted>(Apply);
        On<OrderNumberAssigned>(Apply);
        On<OrderAssigned>(Apply);
        On<OrderAcknowledged>(Apply);
        On<ClientDocumentRequested>(Apply);
        On<RequestedDocumentProvided>(Apply);
        On<DescriptionOfWorkSubmitted>(Apply);
        On<DescriptionOfWorkApproved>(Apply);
        On<VendorAssigned>(Apply);
        On<InspectionScheduled>(Apply);
        On<InspectionCompleted>(Apply);
        On<ReportDelivered>(Apply);
    }

    /// <summary>Whether the client submitted the order; staff don't see drafts.</summary>
    [Id(0)] public bool Submitted { get; set; }

    [Id(1)] public StaffOrderStatus Status { get; set; }

    /// <summary>The client the order is for, as a tenant-qualified client ID.</summary>
    [Id(2)] public string? ClientId { get; set; }

    /// <summary>The client user who submitted the order, as a tenant user ID.</summary>
    [Id(3)] public string? EnteredBy { get; set; }

    [Id(4)] public string? FormTypeName { get; set; }

    /// <summary>The property address on one line.</summary>
    [Id(5)] public string? PropertyAddress { get; set; }

    /// <summary>When the client gets the report, or null until the order is assigned to a vendor.</summary>
    [Id(6)] public DateOnly? DueDate { get; set; }

    [Id(7)] public string? AccessContactName { get; set; }

    [Id(8)] public string? AccessContactPhone { get; set; }

    [Id(9)] public long? OrderNumber { get; set; }

    /// <summary>The staff member the order is assigned to, as a tenant user ID.</summary>
    [Id(10)] public string? AssignedTo { get; set; }

    /// <summary>What happened to the order, oldest first, with the IDs the query turns into names.</summary>
    [Id(11)] public List<OrderTimelineItem> Timeline { get; init; } = [];

    /// <summary>The documents requested from the client that it has not provided yet, oldest first.</summary>
    [Id(12)] public List<OrderDocumentRequestSummary> OpenRequests { get; init; } = [];

    /// <summary>The order's supporting documents, in the order they were added, with the IDs the query turns into names.</summary>
    [Id(13)] public List<OrderDocumentItem> Documents { get; init; } = [];

    /// <summary>The latest version of the order's description of work, or null until one is submitted.</summary>
    [Id(14)] public DescriptionOfWorkSummary? DescriptionOfWork { get; set; }

    /// <summary>The vendor the order is assigned to, or null until it is assigned to one.</summary>
    [Id(15)] public OrderVendorItem? Vendor { get; set; }

    /// <summary>What the client pays the AMC, or null until the order is assigned to a vendor.</summary>
    [Id(16)] public decimal? ServiceFee { get; set; }

    /// <summary>The property's inspection, or null until the vendor schedules it.</summary>
    [Id(17)] public OrderInspectionSummary? Inspection { get; set; }

    /// <summary>The vendor's completed report, or null until the vendor delivers it.</summary>
    [Id(18)] public OrderReportSummary? Report { get; set; }

    private bool Apply(OrderDraftStarted evt, IStreamedEvent<object> streamedEvent)
    {
        ClientId = evt.ClientId;
        FormTypeName = evt.FormTypeName;
        PropertyAddress = PropertyAddressLine.Format(evt.PropertyStreet, evt.PropertyCity, evt.PropertyState, evt.PropertyZip);
        return true;
    }

    private bool Apply(RequestAndPropertySaved evt, IStreamedEvent<object> streamedEvent)
    {
        FormTypeName = evt.FormTypeName;
        PropertyAddress = PropertyAddressLine.Format(evt.PropertyStreet, evt.PropertyCity, evt.PropertyState, evt.PropertyZip);
        return true;
    }

    private bool Apply(BorrowerAndAccessContactSaved evt, IStreamedEvent<object> streamedEvent)
    {
        AccessContactName = evt.AccessContactName;
        AccessContactPhone = evt.AccessContactPhone;
        return true;
    }

    private bool Apply(SupportingDocumentAttached evt, IStreamedEvent<object> streamedEvent)
    {
        Documents.Add(new OrderDocumentItem(evt.DocumentId, evt.FileName, evt.DocumentTypeName, evt.InitiatedBy, evt.OccurredAt));
        return true;
    }

    private bool Apply(SupportingDocumentRemoved evt, IStreamedEvent<object> streamedEvent)
        => Documents.RemoveAll(document => document.DocumentId == evt.DocumentId) > 0;

    private bool Apply(OrderSubmitted evt, IStreamedEvent<object> streamedEvent)
    {
        Submitted = true;
        Status = StaffOrderStatus.Submitted;
        EnteredBy = evt.InitiatedBy;
        Timeline.Add(new OrderTimelineItem(evt.OccurredAt, OrderTimelineKind.Submitted, evt.InitiatedBy, null));
        return true;
    }

    private bool Apply(OrderNumberAssigned evt, IStreamedEvent<object> streamedEvent)
    {
        OrderNumber = evt.OrderNumber;
        Timeline.Add(new OrderTimelineItem(evt.OccurredAt, OrderTimelineKind.OrderNumberAssigned, null, evt.OrderNumber));
        return true;
    }

    private bool Apply(OrderAssigned evt, IStreamedEvent<object> streamedEvent)
    {
        Status = StaffOrderStatus.Assigned;
        AssignedTo = evt.AssignedToUserId;
        Timeline.Add(new OrderTimelineItem(evt.OccurredAt, OrderTimelineKind.Assigned, evt.AssignedToUserId, null));
        return true;
    }

    private bool Apply(OrderAcknowledged evt, IStreamedEvent<object> streamedEvent)
    {
        Status = StaffOrderStatus.Acknowledged;
        Timeline.Add(new OrderTimelineItem(evt.OccurredAt, OrderTimelineKind.Acknowledged, evt.InitiatedBy, null));
        return true;
    }

    private bool Apply(ClientDocumentRequested evt, IStreamedEvent<object> streamedEvent)
    {
        OpenRequests.Add(new OrderDocumentRequestSummary(evt.RequestId, evt.DocumentTypeName, evt.Message, evt.OccurredAt));
        Timeline.Add(new OrderTimelineItem(
            evt.OccurredAt, OrderTimelineKind.DocumentRequested, evt.InitiatedBy, null, evt.DocumentTypeName));
        return true;
    }

    private bool Apply(RequestedDocumentProvided evt, IStreamedEvent<object> streamedEvent)
    {
        OpenRequests.RemoveAll(request => request.RequestId == evt.RequestId);
        Documents.Add(new OrderDocumentItem(evt.DocumentId, evt.FileName, evt.DocumentTypeName, evt.InitiatedBy, evt.OccurredAt));
        Timeline.Add(new OrderTimelineItem(
            evt.OccurredAt, OrderTimelineKind.DocumentProvided, evt.InitiatedBy, null, evt.DocumentTypeName));
        return true;
    }

    private bool Apply(DescriptionOfWorkSubmitted evt, IStreamedEvent<object> streamedEvent)
    {
        Status = StaffOrderStatus.DescriptionOfWorkAwaitingApproval;
        DescriptionOfWork = new DescriptionOfWorkSummary(
            evt.DescriptionOfWorkId, evt.DescriptionOfWorkVersion, evt.DescriptionOfWorkHtml, Approved: false, evt.OccurredAt);
        Timeline.Add(new OrderTimelineItem(evt.OccurredAt, OrderTimelineKind.DescriptionOfWorkSubmitted, evt.InitiatedBy, null,
            DescriptionOfWorkVersion: evt.DescriptionOfWorkVersion));
        return true;
    }

    private bool Apply(DescriptionOfWorkApproved evt, IStreamedEvent<object> streamedEvent)
    {
        Status = StaffOrderStatus.DescriptionOfWorkApproved;
        DescriptionOfWork = DescriptionOfWork is null ? null : DescriptionOfWork with { Approved = true };
        Timeline.Add(new OrderTimelineItem(evt.OccurredAt, OrderTimelineKind.DescriptionOfWorkApproved, evt.InitiatedBy, null,
            DescriptionOfWorkVersion: evt.DescriptionOfWorkVersion));
        return true;
    }

    private bool Apply(VendorAssigned evt, IStreamedEvent<object> streamedEvent)
    {
        Status = StaffOrderStatus.VendorAssigned;
        Vendor = new OrderVendorItem(evt.VendorId, evt.VendorFee, evt.VendorDueDate);
        ServiceFee = evt.ServiceFee;
        DueDate = evt.DueDate;
        Timeline.Add(new OrderTimelineItem(evt.OccurredAt, OrderTimelineKind.VendorAssigned, evt.InitiatedBy, null,
            VendorId: evt.VendorId));
        return true;
    }

    private bool Apply(InspectionScheduled evt, IStreamedEvent<object> streamedEvent)
    {
        Status = StaffOrderStatus.InspectionScheduled;
        Inspection = new OrderInspectionSummary(evt.InspectionAt, evt.ContactName, CompletedOn: null);
        Timeline.Add(new OrderTimelineItem(evt.OccurredAt, OrderTimelineKind.InspectionScheduled, evt.InitiatedBy, null));
        return true;
    }

    private bool Apply(InspectionCompleted evt, IStreamedEvent<object> streamedEvent)
    {
        Status = StaffOrderStatus.InspectionCompleted;
        Inspection = Inspection is null ? null : Inspection with { CompletedOn = evt.CompletedOn };
        Timeline.Add(new OrderTimelineItem(evt.OccurredAt, OrderTimelineKind.InspectionCompleted, evt.InitiatedBy, null));
        return true;
    }

    private bool Apply(ReportDelivered evt, IStreamedEvent<object> streamedEvent)
    {
        Status = StaffOrderStatus.ReportDelivered;
        Report = new OrderReportSummary(evt.DocumentId, evt.FileName, evt.OccurredAt);
        Timeline.Add(new OrderTimelineItem(evt.OccurredAt, OrderTimelineKind.ReportDelivered, evt.InitiatedBy, null));
        return true;
    }
}

/// <summary>The vendor an order is assigned to as the staff order page's projection stores it.</summary>
/// <param name="VendorId">The vendor, as a tenant-qualified vendor ID, which the query turns into its name.</param>
/// <param name="VendorFee">What the AMC pays the vendor for the report.</param>
/// <param name="VendorDueDate">When the AMC needs the vendor's report.</param>
[GenerateSerializer, Immutable]
public sealed record OrderVendorItem(
    string VendorId,
    decimal VendorFee,
    DateOnly VendorDueDate);

/// <summary>A supporting document of the order as the staff order page's projection stores it.</summary>
/// <param name="DocumentId">The document.</param>
/// <param name="FileName">The name of its file.</param>
/// <param name="DocumentTypeName">What kind of document it is.</param>
/// <param name="AddedBy">Who attached or provided it, as a tenant user ID.</param>
/// <param name="AddedAt">When it was added.</param>
[GenerateSerializer, Immutable]
public sealed record OrderDocumentItem(
    string DocumentId,
    string FileName,
    string DocumentTypeName,
    string? AddedBy,
    DateTimeOffset AddedAt);
