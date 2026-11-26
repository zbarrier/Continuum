using System.Net;

using Continuum.CSharpFunctionalExtensions;
using Continuum.CSharpFunctionalExtensions.Orleans;
using Continuum.Orleans.Tenancy;
using Continuum.Streaming;
using Continuum.Streaming.Orleans;

using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Abstractions.ReadModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace GetRDone.Orders.Grains.ReadModels;

/// <summary>
///     Projects the vendor order page of one order from the order's own stream (see <see cref="OrderProjections"/>).
/// </summary>
/// <remarks>
///     The stream provider activates the grain whose key is the stream key, the order ID, so there is one instance per
///     order. <see cref="AuthorizeAttribute"/> on the class covers the query and
///     <see cref="IProjectionGrain.OnNextBatchAsync"/>, which a catch-up subscription calls signed in as its automation
///     account; stream deliveries arrive through Orleans' stream consumer extension, which it does not cover.
/// </remarks>
[ImplicitStreamSubscription(OrderProjections.OrderEventsNamespace)]
[GrainType("vendorOrderDetails")]
[Authorize]
public sealed class VendorOrderDetailsGrain(
    [PersistentState("vendorOrderDetails", OrderProjections.StorageName)] IPersistentState<VendorOrderDetailsState> storage,
    ILogger<VendorOrderDetailsGrain> logger)
    : ProjectionGrain<VendorOrderDetailsGrain, VendorOrderDetailsState>(storage, logger), IVendorOrderDetailsGrain
{
    public async Task<Result<VendorOrderDetails>> GetVendorOrderDetailsAsync()
    {
        TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);
        return await Result.Success()
            .Ensure(() => State.Vendor is not null, () => RequestErrors.NewNotFound("The order was not found."))
            .BindTry(async () =>
            {
                var names = await UserDirectoryNames.LoadAsync(GrainFactory, tenantId);

                return this.GetCurrentVendorId(names, tenantId)
                    .Ensure(vendorId => vendorId == State.Vendor!.VendorId,
                        _ => RequestErrors.New(HttpStatusCode.Forbidden, Grpc.Core.StatusCode.PermissionDenied,
                            "You can only see orders assigned to your vendor."))
                    .Map(_ => new VendorOrderDetails(
                        this.GetPrimaryKeyString(),
                        State.OrderNumber,
                        State.Status,
                        State.ClientId is null ? null : names.ClientName(State.ClientId),
                        State.FormTypeName!,
                        State.PropertyAddress!,
                        State.AccessContactName,
                        State.AccessContactPhone,
                        State.Vendor!.VendorFee,
                        State.Vendor.VendorDueDate,
                        State.DescriptionOfWork!,
                        State.Inspection,
                        State.Report,
                        OrderTimeline.Describe(State.Timeline, names, OrderTimelineAudience.Vendor),
                        GetAllowedActions()));
            }, ex => GlobalGrainErrorHandler.Default(ex, logger));
    }

    private VendorOrderActions GetAllowedActions() => State.Status switch
    {
        VendorOrderStatus.Assigned => VendorOrderActions.ScheduleInspection,
        VendorOrderStatus.InspectionScheduled => VendorOrderActions.CompleteInspection,
        VendorOrderStatus.InspectionCompleted => VendorOrderActions.DeliverReport,
        _ => VendorOrderActions.None
    };
}

[GenerateSerializer]
public sealed class VendorOrderDetailsState : HandlerProjectionState<VendorOrderDetailsState>
{
    public VendorOrderDetailsState()
    {
        On<OrderDraftStarted>(Apply);
        On<RequestAndPropertySaved>(Apply);
        On<BorrowerAndAccessContactSaved>(Apply);
        On<OrderNumberAssigned>(Apply);
        On<DescriptionOfWorkSubmitted>(Apply);
        On<DescriptionOfWorkApproved>(Apply);
        On<VendorAssigned>(Apply);
        On<InspectionScheduled>(Apply);
        On<InspectionCompleted>(Apply);
        On<ReportDelivered>(Apply);
    }

    /// <summary>The client the order is for, as a tenant-qualified client ID.</summary>
    [Id(0)] public string? ClientId { get; set; }

    [Id(1)] public long? OrderNumber { get; set; }

    [Id(2)] public string? FormTypeName { get; set; }

    /// <summary>The property address on one line.</summary>
    [Id(3)] public string? PropertyAddress { get; set; }

    [Id(4)] public string? AccessContactName { get; set; }

    [Id(5)] public string? AccessContactPhone { get; set; }

    /// <summary>The latest version of the order's description of work; the vendor sees the approved one.</summary>
    [Id(6)] public DescriptionOfWorkSummary? DescriptionOfWork { get; set; }

    /// <summary>The vendor the order is assigned to, or null until it is assigned to one; only its users can read the page.</summary>
    [Id(7)] public OrderVendorItem? Vendor { get; set; }

    /// <summary>What happened to the order since it was assigned to the vendor, oldest first.</summary>
    [Id(8)] public List<OrderTimelineItem> Timeline { get; init; } = [];

    [Id(9)] public VendorOrderStatus Status { get; set; }

    /// <summary>The property's inspection, or null until the vendor schedules it.</summary>
    [Id(10)] public OrderInspectionSummary? Inspection { get; set; }

    /// <summary>The vendor's completed report, or null until the vendor delivers it.</summary>
    [Id(11)] public OrderReportSummary? Report { get; set; }

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

    private bool Apply(OrderNumberAssigned evt, IStreamedEvent<object> streamedEvent)
    {
        OrderNumber = evt.OrderNumber;
        if (Vendor is not null)
        {
            Timeline.Add(new OrderTimelineItem(evt.OccurredAt, OrderTimelineKind.OrderNumberAssigned, null, evt.OrderNumber));
        }
        return true;
    }

    private bool Apply(DescriptionOfWorkSubmitted evt, IStreamedEvent<object> streamedEvent)
    {
        DescriptionOfWork = new DescriptionOfWorkSummary(
            evt.DescriptionOfWorkId, evt.DescriptionOfWorkVersion, evt.DescriptionOfWorkHtml, Approved: false, evt.OccurredAt);
        return true;
    }

    private bool Apply(DescriptionOfWorkApproved evt, IStreamedEvent<object> streamedEvent)
    {
        DescriptionOfWork = DescriptionOfWork is null ? null : DescriptionOfWork with { Approved = true };
        return true;
    }

    private bool Apply(VendorAssigned evt, IStreamedEvent<object> streamedEvent)
    {
        Status = VendorOrderStatus.Assigned;
        Vendor = new OrderVendorItem(evt.VendorId, evt.VendorFee, evt.VendorDueDate);
        Timeline.Add(new OrderTimelineItem(evt.OccurredAt, OrderTimelineKind.VendorAssigned, evt.InitiatedBy, null,
            VendorId: evt.VendorId));
        return true;
    }

    private bool Apply(InspectionScheduled evt, IStreamedEvent<object> streamedEvent)
    {
        Status = VendorOrderStatus.InspectionScheduled;
        Inspection = new OrderInspectionSummary(evt.InspectionAt, evt.ContactName, CompletedOn: null);
        Timeline.Add(new OrderTimelineItem(evt.OccurredAt, OrderTimelineKind.InspectionScheduled, evt.InitiatedBy, null));
        return true;
    }

    private bool Apply(InspectionCompleted evt, IStreamedEvent<object> streamedEvent)
    {
        Status = VendorOrderStatus.InspectionCompleted;
        Inspection = Inspection is null ? null : Inspection with { CompletedOn = evt.CompletedOn };
        Timeline.Add(new OrderTimelineItem(evt.OccurredAt, OrderTimelineKind.InspectionCompleted, evt.InitiatedBy, null));
        return true;
    }

    private bool Apply(ReportDelivered evt, IStreamedEvent<object> streamedEvent)
    {
        Status = VendorOrderStatus.ReportDelivered;
        Report = new OrderReportSummary(evt.DocumentId, evt.FileName, evt.OccurredAt);
        Timeline.Add(new OrderTimelineItem(evt.OccurredAt, OrderTimelineKind.ReportDelivered, evt.InitiatedBy, null));
        return true;
    }
}
