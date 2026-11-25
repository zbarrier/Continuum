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
                    State.AccessContactName,
                    State.AccessContactPhone,
                    State.AssignedTo,
                    names.UserName(State.AssignedTo),
                    State.OpenRequests.ToArray(),
                    OrderTimeline.Describe(State.Timeline, names),
                    GetAllowedActions(tenantId)));
            }, ex => GlobalGrainErrorHandler.Default(ex, logger));
    }

    private OrderActions GetAllowedActions(string? tenantId) => State.Status switch
    {
        StaffOrderStatus.Submitted when this.GetCurrentUser().IsInAnyRole(StaffRoles.OrderAssigners) => OrderActions.AssignToMe,
        StaffOrderStatus.Assigned when State.AssignedTo == this.GetCurrentTenantUserId(tenantId) => OrderActions.Acknowledge,
        StaffOrderStatus.Acknowledged when State.AssignedTo == this.GetCurrentTenantUserId(tenantId)
            || this.GetCurrentUser().IsInAnyRole(StaffRoles.OrderAssigners) => OrderActions.RequestDocument,
        _ => OrderActions.None
    };
}

[GenerateSerializer]
public sealed class OrderDetailsState : HandlerProjectionState<OrderDetailsState>
{
    public OrderDetailsState()
    {
        On<OrderDraftStarted>(Apply);
        On<RequestAndPropertySaved>(Apply);
        On<BorrowerAndAccessContactSaved>(Apply);
        On<OrderSubmitted>(Apply);
        On<OrderNumberAssigned>(Apply);
        On<OrderAssigned>(Apply);
        On<OrderAcknowledged>(Apply);
        On<ClientDocumentRequested>(Apply);
        On<RequestedDocumentProvided>(Apply);
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

    [Id(6)] public DateOnly DueDate { get; set; }

    [Id(7)] public string? AccessContactName { get; set; }

    [Id(8)] public string? AccessContactPhone { get; set; }

    [Id(9)] public long? OrderNumber { get; set; }

    /// <summary>The staff member the order is assigned to, as a tenant user ID.</summary>
    [Id(10)] public string? AssignedTo { get; set; }

    /// <summary>What happened to the order, oldest first, with the IDs the query turns into names.</summary>
    [Id(11)] public List<OrderTimelineItem> Timeline { get; init; } = [];

    /// <summary>The documents requested from the client that it has not provided yet, oldest first.</summary>
    [Id(12)] public List<OrderDocumentRequestSummary> OpenRequests { get; init; } = [];

    private bool Apply(OrderDraftStarted evt, IStreamedEvent<object> streamedEvent)
    {
        ClientId = evt.ClientId;
        FormTypeName = evt.FormTypeName;
        PropertyAddress = PropertyAddressLine.Format(evt.PropertyStreet, evt.PropertyCity, evt.PropertyState, evt.PropertyZip);
        DueDate = evt.DueDate;
        return true;
    }

    private bool Apply(RequestAndPropertySaved evt, IStreamedEvent<object> streamedEvent)
    {
        FormTypeName = evt.FormTypeName;
        PropertyAddress = PropertyAddressLine.Format(evt.PropertyStreet, evt.PropertyCity, evt.PropertyState, evt.PropertyZip);
        DueDate = evt.DueDate;
        return true;
    }

    private bool Apply(BorrowerAndAccessContactSaved evt, IStreamedEvent<object> streamedEvent)
    {
        AccessContactName = evt.AccessContactName;
        AccessContactPhone = evt.AccessContactPhone;
        return true;
    }

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
        Timeline.Add(new OrderTimelineItem(
            evt.OccurredAt, OrderTimelineKind.DocumentProvided, evt.InitiatedBy, null, evt.DocumentTypeName));
        return true;
    }
}
