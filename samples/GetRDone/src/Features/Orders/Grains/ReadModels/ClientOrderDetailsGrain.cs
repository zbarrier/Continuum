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
///     Projects the client order page of one order from the order's own stream (see <see cref="OrderProjections"/>).
/// </summary>
/// <remarks>
///     The stream provider activates the grain whose key is the stream key, the order ID, so there is one instance per
///     order. <see cref="AuthorizeAttribute"/> on the class covers the query and
///     <see cref="IProjectionGrain.OnNextBatchAsync"/>, which a catch-up subscription calls signed in as its automation
///     account; stream deliveries arrive through Orleans' stream consumer extension, which it does not cover.
/// </remarks>
[ImplicitStreamSubscription(OrderProjections.OrderEventsNamespace)]
[GrainType("clientOrderDetails")]
[Authorize]
public sealed class ClientOrderDetailsGrain(
    [PersistentState("clientOrderDetails", OrderProjections.StorageName)] IPersistentState<ClientOrderDetailsState> storage,
    ILogger<ClientOrderDetailsGrain> logger)
    : ProjectionGrain<ClientOrderDetailsGrain, ClientOrderDetailsState>(storage, logger), IClientOrderDetailsGrain
{
    public async Task<Result<ClientOrderDetails>> GetClientOrderDetailsAsync()
    {
        TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);
        return await Result.Success()
            .Ensure(() => State.Submitted, () => RequestErrors.NewNotFound("The order was not found."))
            .Ensure(() => State.StartedBy == this.GetCurrentTenantUserId(tenantId),
                () => RequestErrors.New(HttpStatusCode.Forbidden, Grpc.Core.StatusCode.PermissionDenied,
                    "You can only see orders you started."))
            .BindTry(async () =>
            {
                var names = await UserDirectoryNames.LoadAsync(GrainFactory, tenantId);

                return Result.Success(new ClientOrderDetails(
                    this.GetPrimaryKeyString(),
                    State.OrderNumber,
                    State.Status,
                    State.FormTypeName!,
                    State.PropertyAddress!,
                    State.DueDate,
                    State.ServiceFee,
                    names.UserName(State.AssignedTo),
                    State.OpenRequests.ToArray(),
                    State.Documents.ToArray(),
                    State.DescriptionOfWork,
                    OrderTimeline.Describe(State.Timeline, names, OrderTimelineAudience.Client),
                    GetAllowedActions()));
            }, ex => GlobalGrainErrorHandler.Default(ex, logger));
    }

    private ClientOrderActions GetAllowedActions()
        => (State.OpenRequests.Count > 0 ? ClientOrderActions.UploadRequestedDocument : ClientOrderActions.None)
            | (State.Status == ClientOrderStatus.DescriptionOfWorkAwaitingApproval
                ? ClientOrderActions.ApproveDescriptionOfWork
                : ClientOrderActions.None);
}

[GenerateSerializer]
public sealed class ClientOrderDetailsState : HandlerProjectionState<ClientOrderDetailsState>
{
    public ClientOrderDetailsState()
    {
        On<OrderDraftStarted>(Apply);
        On<RequestAndPropertySaved>(Apply);
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
    }

    /// <summary>Whether the client submitted the order; drafts have their own page.</summary>
    [Id(0)] public bool Submitted { get; set; }

    /// <summary>The client user who started the order, as a tenant user ID; only they can read the page.</summary>
    [Id(1)] public string? StartedBy { get; set; }

    [Id(2)] public ClientOrderStatus Status { get; set; }

    [Id(3)] public long? OrderNumber { get; set; }

    [Id(4)] public string? FormTypeName { get; set; }

    /// <summary>The property address on one line.</summary>
    [Id(5)] public string? PropertyAddress { get; set; }

    /// <summary>When the client gets the report, or null until the order is assigned to an appraiser.</summary>
    [Id(6)] public DateOnly? DueDate { get; set; }

    /// <summary>The staff member the order is assigned to, as a tenant user ID.</summary>
    [Id(7)] public string? AssignedTo { get; set; }

    /// <summary>The documents the AMC requested that the client has not provided yet, oldest first.</summary>
    [Id(8)] public List<ClientDocumentRequest> OpenRequests { get; init; } = [];

    /// <summary>The order's supporting documents, in the order they were added.</summary>
    [Id(9)] public List<ClientOrderDocument> Documents { get; init; } = [];

    /// <summary>What happened to the order, oldest first, with the IDs the query turns into names.</summary>
    [Id(10)] public List<OrderTimelineItem> Timeline { get; init; } = [];

    /// <summary>The latest version of the order's description of work, or null until one is submitted.</summary>
    [Id(11)] public DescriptionOfWorkSummary? DescriptionOfWork { get; set; }

    /// <summary>What the client pays the AMC, or null until the order is assigned to an appraiser.</summary>
    [Id(12)] public decimal? ServiceFee { get; set; }

    private bool Apply(OrderDraftStarted evt, IStreamedEvent<object> streamedEvent)
    {
        StartedBy = evt.InitiatedBy;
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

    private bool Apply(SupportingDocumentAttached evt, IStreamedEvent<object> streamedEvent)
    {
        Documents.Add(new ClientOrderDocument(evt.DocumentId, evt.FileName, evt.DocumentTypeName));
        return true;
    }

    private bool Apply(SupportingDocumentRemoved evt, IStreamedEvent<object> streamedEvent)
        => Documents.RemoveAll(document => document.DocumentId == evt.DocumentId) > 0;

    private bool Apply(OrderSubmitted evt, IStreamedEvent<object> streamedEvent)
    {
        Submitted = true;
        Status = ClientOrderStatus.Submitted;
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
        Status = ClientOrderStatus.Assigned;
        AssignedTo = evt.AssignedToUserId;
        Timeline.Add(new OrderTimelineItem(evt.OccurredAt, OrderTimelineKind.Assigned, evt.AssignedToUserId, null));
        return true;
    }

    private bool Apply(OrderAcknowledged evt, IStreamedEvent<object> streamedEvent)
    {
        Status = ClientOrderStatus.Acknowledged;
        Timeline.Add(new OrderTimelineItem(evt.OccurredAt, OrderTimelineKind.Acknowledged, evt.InitiatedBy, null));
        return true;
    }

    private bool Apply(ClientDocumentRequested evt, IStreamedEvent<object> streamedEvent)
    {
        OpenRequests.Add(new ClientDocumentRequest(evt.RequestId, evt.DocumentTypeName, evt.Message, evt.OccurredAt));
        Timeline.Add(new OrderTimelineItem(
            evt.OccurredAt, OrderTimelineKind.DocumentRequested, evt.InitiatedBy, null, evt.DocumentTypeName));
        return true;
    }

    private bool Apply(RequestedDocumentProvided evt, IStreamedEvent<object> streamedEvent)
    {
        OpenRequests.RemoveAll(request => request.RequestId == evt.RequestId);
        Documents.Add(new ClientOrderDocument(evt.DocumentId, evt.FileName, evt.DocumentTypeName));
        Timeline.Add(new OrderTimelineItem(
            evt.OccurredAt, OrderTimelineKind.DocumentProvided, evt.InitiatedBy, null, evt.DocumentTypeName));
        return true;
    }

    private bool Apply(DescriptionOfWorkSubmitted evt, IStreamedEvent<object> streamedEvent)
    {
        Status = ClientOrderStatus.DescriptionOfWorkAwaitingApproval;
        DescriptionOfWork = new DescriptionOfWorkSummary(
            evt.DescriptionOfWorkId, evt.DescriptionOfWorkVersion, evt.DescriptionOfWorkHtml, Approved: false, evt.OccurredAt);
        Timeline.Add(new OrderTimelineItem(evt.OccurredAt, OrderTimelineKind.DescriptionOfWorkSubmitted, evt.InitiatedBy, null,
            DescriptionOfWorkVersion: evt.DescriptionOfWorkVersion));
        return true;
    }

    private bool Apply(DescriptionOfWorkApproved evt, IStreamedEvent<object> streamedEvent)
    {
        Status = ClientOrderStatus.DescriptionOfWorkApproved;
        DescriptionOfWork = DescriptionOfWork is null ? null : DescriptionOfWork with { Approved = true };
        Timeline.Add(new OrderTimelineItem(evt.OccurredAt, OrderTimelineKind.DescriptionOfWorkApproved, evt.InitiatedBy, null,
            DescriptionOfWorkVersion: evt.DescriptionOfWorkVersion));
        return true;
    }

    private bool Apply(VendorAssigned evt, IStreamedEvent<object> streamedEvent)
    {
        Status = ClientOrderStatus.VendorAssigned;
        ServiceFee = evt.ServiceFee;
        DueDate = evt.DueDate;
        Timeline.Add(new OrderTimelineItem(evt.OccurredAt, OrderTimelineKind.VendorAssigned, evt.InitiatedBy, null,
            VendorId: evt.VendorId));
        return true;
    }
}
