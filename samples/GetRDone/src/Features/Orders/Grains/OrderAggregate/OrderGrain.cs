using Continuum.CSharpFunctionalExtensions;
using Continuum.CSharpFunctionalExtensions.Orleans;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Events;
using Continuum.Orleans.Tenancy;
using Continuum.Streaming;

using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Abstractions.OrderNumbers;
using GetRDone.Orders.Abstractions.ReadModels;
using GetRDone.Orders.Grains.ReadModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

using Orleans.Providers;
using Orleans.Streams;
using Orleans.Streams.Core;

namespace GetRDone.Orders.Grains.OrderAggregate;

/// <remarks>
///     The order reacts to the events that other grains send to it as a stream target, which the
///     <see cref="OrderReactors"/> stream provider delivers on <see cref="OrderReactors.OrderNamespace"/>. Stream
///     deliveries arrive through Orleans' stream consumer extension, which <see cref="AuthorizeAttribute"/> does not
///     cover, so each reaction signs in as its automation account before it runs a command.
/// </remarks>
[Authorize]
[GrainType(OrderReactors.OrderCategory)]
[ImplicitStreamSubscription(OrderReactors.OrderNamespace)]
[LogConsistencyProvider(ProviderName = "EventLog")]
public sealed partial class OrderGrain
    : EventDrivenStateMachineGrain<OrderState, OrderEvent, OrderStatus, OrderCommand>, IOrderGrain, IStreamSubscriptionObserver
{
    private readonly IOrderPermissions _permissions;
    private readonly ILogger<OrderGrain> _logger;
    private readonly Func<Exception, Error> _grainCallErrorHandler;

    public OrderGrain(IServiceProvider serviceProvider, IOrderPermissions permissions, ILogger<OrderGrain> logger)
        : base(serviceProvider, "kurrentdb")
    {
        _permissions = permissions;
        _logger = logger;
        _grainCallErrorHandler = ex => GlobalGrainErrorHandler.Default(ex, _logger);
    }

    /// <summary>
    ///     Sends every order event to the stream of the user who started the order and to its tenant's orders stream as well,
    ///     so the client user's read models (such as <see cref="ClientOrderDraftsGrain"/> and <see cref="ClientOrdersGrain"/>)
    ///     see all of that user's orders on one stream, and the staff dashboard (<see cref="UnassignedOrdersGrain"/>,
    ///     <see cref="InProgressOrdersGrain"/> and <see cref="ShippedOrdersGrain"/>) all of the tenant's orders.
    /// </summary>
    protected override IReadOnlyList<StreamTarget> GetStreamTargets(object @event)
    {
        TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);
        // The draft started event is what records the starter, so it is not in the state yet while it is appended.
        var startedBy = @event is OrderDraftStarted started ? started.InitiatedBy : State.StartedBy;

        List<StreamTarget> targets = [];
        if (startedBy is not null)
        {
            targets.Add(StreamTarget.Transaction(OrderProjections.ClientUserOrdersStreamTarget(startedBy)));
        }
        if (tenantId is not null)
        {
            targets.Add(StreamTarget.Transaction(OrderProjections.TenantOrdersStreamTarget(tenantId)));
        }
        return targets;
    }

    /// <summary>Attaches to the subscription the runtime created for the order's reactions.</summary>
    public async Task OnSubscribed(IStreamSubscriptionHandleFactory handleFactory)
        => await handleFactory.Create<IStreamedEvent<object>>().ResumeAsync((streamedEvent, _) => ReactAsync(streamedEvent.Event));

    /// <summary>
    ///     Runs the reaction to an event delivered to the order. A reaction that throws has the event delivered again until
    ///     it succeeds, so it throws only for failures that may pass.
    /// </summary>
    private Task ReactAsync(object @event) => @event switch
    {
        OrderNumberIssued issued => ReactToOrderNumberIssuedAsync(issued),
        _ => Task.CompletedTask
    };
}

public enum OrderStatus
{
    None,
    Draft,
    Submitted,
    Assigned,
    Acknowledged,
    DescriptionOfWorkAwaitingApproval,
    DescriptionOfWorkApproved,
    VendorAssigned,
    InspectionScheduled,
    InspectionCompleted,
    ReportDelivered,
    Shipped
}

public enum OrderCommand
{
    StartDraft,
    SaveRequestAndProperty,
    SaveBorrowerAndAccessContact,
    AttachSupportingDocument,
    RemoveSupportingDocument,
    SubmitOrder,
    AssignOrderNumber,
    AssignOrder,
    AcknowledgeOrder,
    RequestClientDocument,
    ProvideRequestedDocument,
    SubmitDescriptionOfWork,
    ApproveDescriptionOfWork,
    AssignVendor,
    ScheduleInspection,
    CompleteInspection,
    DeliverReport,
    ShipOrder
}

[GenerateSerializer]
public sealed partial class OrderState : EventDrivenStateMachineState<OrderState, OrderEvent, OrderStatus, OrderCommand>
{
    public OrderState() : base(OrderStatus.None) { }

    /// <summary>The user who started the order, as a tenant-qualified user ID.</summary>
    [Id(0)] public string? StartedBy { get; private set; }

    /// <summary>
    ///     The blob path of each document attached to the order, by document ID, so a document can't be attached twice and its
    ///     removal records where its file is.
    /// </summary>
    [Id(1)] public Dictionary<string, string> SupportingDocumentBlobPaths { get; private set; } = [];

    /// <summary>The client the order is for, as a tenant-qualified client ID.</summary>
    [Id(2)] public string? ClientId { get; private set; }

    /// <summary>Whether the Borrower &amp; Access contact tab was saved, which the order needs to be submitted.</summary>
    [Id(3)] public bool HasBorrower { get; private set; }

    /// <summary>The order's number, or null until the order is given the number the generator issued it.</summary>
    [Id(4)] public long? OrderNumber { get; private set; }

    /// <summary>Whether the New Order Notifier emailed the new order notice, so it is sent only once.</summary>
    [Id(5)] public bool NewOrderNoticeSent { get; private set; }

    /// <summary>The staff member the order is assigned to, as a tenant-qualified user ID, or null until it is assigned.</summary>
    [Id(6)] public string? AssignedTo { get; private set; }

    /// <summary>The documents the AMC requested from the client, by request ID.</summary>
    [Id(7)] public Dictionary<string, OrderDocumentRequest> DocumentRequests { get; private set; } = [];

    /// <summary>The order's description of work and how far it is, or null until it is first submitted.</summary>
    [Id(8)] public OrderDescriptionOfWork? DescriptionOfWork { get; private set; }

    /// <summary>The vendor the order is assigned to and whether its users were told, or null until it is assigned to one.</summary>
    [Id(9)] public OrderVendorAssignment? VendorAssignment { get; private set; }

    /// <summary>When the vendor's appraiser inspects the property, or null until the vendor schedules it.</summary>
    [Id(10)] public DateTimeOffset? InspectionAt { get; private set; }

    /// <summary>The date the vendor's appraiser inspected the property, or null until the vendor records it.</summary>
    [Id(11)] public DateOnly? InspectionCompletedOn { get; private set; }

    /// <summary>The vendor's completed report and whether the assigned staff member was told, or null until it is delivered.</summary>
    [Id(12)] public OrderReport? Report { get; private set; }

    /// <summary>Whether the Order Shipped Notifier told the client user who entered the order that its report is ready.</summary>
    [Id(13)] public bool OrderShippedNoticeSent { get; private set; }

    protected override void ConfigureEventHandlers()
    {
        On<OrderDraftStarted>(Handle);
        On<BorrowerAndAccessContactSaved>(Handle);
        On<SupportingDocumentAttached>(Handle);
        On<SupportingDocumentRemoved>(Handle);
        On<OrderSubmitted>(Handle);
        On<OrderNumberAssigned>(Handle);
        On<NewOrderNoticeSent>(Handle);
        On<OrderAssigned>(Handle);
        On<OrderAcknowledged>(Handle);
        On<ClientDocumentRequested>(Handle);
        On<DocumentRequestNoticeSent>(Handle);
        On<RequestedDocumentProvided>(Handle);
        On<DescriptionOfWorkSubmitted>(Handle);
        On<DescriptionOfWorkSubmittedNoticeSent>(Handle);
        On<DescriptionOfWorkApproved>(Handle);
        On<DescriptionOfWorkApprovedNoticeSent>(Handle);
        On<VendorAssigned>(Handle);
        On<VendorAssignedNoticeSent>(Handle);
        On<InspectionScheduled>(Handle);
        On<InspectionCompleted>(Handle);
        On<ReportDelivered>(Handle);
        On<ReportDeliveredNoticeSent>(Handle);
        On<OrderShipped>(Handle);
        On<OrderShippedNoticeSent>(Handle);
    }

    /// <remarks>
    ///     The Order Number Assigner can give an order its number after staff have already assigned or acknowledged it, so
    ///     every status after Submitted permits <see cref="OrderCommand.AssignOrderNumber"/>. Documents can be requested and
    ///     provided once the order is acknowledged, whatever the state of its description of work, and the description of
    ///     work can be revised until the client approves it. The order is assigned to a vendor once, after the approval. The
    ///     vendor then schedules the inspection, completes it and delivers its report, each once and in that order; documents
    ///     can still be requested and provided throughout. Shipping the delivered report completes the order: nothing but its
    ///     number can be given to it after that.
    /// </remarks>
    protected override void ConfigureStateMachine()
    {
        StateMachine.Configure(OrderStatus.None).Permit(OrderCommand.StartDraft, OrderStatus.Draft);
        StateMachine.Configure(OrderStatus.Draft)
            .PermitReentry(OrderCommand.SaveRequestAndProperty)
            .PermitReentry(OrderCommand.SaveBorrowerAndAccessContact)
            .PermitReentry(OrderCommand.AttachSupportingDocument)
            .PermitReentry(OrderCommand.RemoveSupportingDocument)
            .Permit(OrderCommand.SubmitOrder, OrderStatus.Submitted);
        StateMachine.Configure(OrderStatus.Submitted)
            .PermitReentry(OrderCommand.AssignOrderNumber)
            .Permit(OrderCommand.AssignOrder, OrderStatus.Assigned);
        StateMachine.Configure(OrderStatus.Assigned)
            .PermitReentry(OrderCommand.AssignOrderNumber)
            .Permit(OrderCommand.AcknowledgeOrder, OrderStatus.Acknowledged);
        StateMachine.Configure(OrderStatus.Acknowledged)
            .PermitReentry(OrderCommand.AssignOrderNumber)
            .PermitReentry(OrderCommand.RequestClientDocument)
            .PermitReentry(OrderCommand.ProvideRequestedDocument)
            .Permit(OrderCommand.SubmitDescriptionOfWork, OrderStatus.DescriptionOfWorkAwaitingApproval);
        StateMachine.Configure(OrderStatus.DescriptionOfWorkAwaitingApproval)
            .PermitReentry(OrderCommand.AssignOrderNumber)
            .PermitReentry(OrderCommand.RequestClientDocument)
            .PermitReentry(OrderCommand.ProvideRequestedDocument)
            .PermitReentry(OrderCommand.SubmitDescriptionOfWork)
            .Permit(OrderCommand.ApproveDescriptionOfWork, OrderStatus.DescriptionOfWorkApproved);
        StateMachine.Configure(OrderStatus.DescriptionOfWorkApproved)
            .PermitReentry(OrderCommand.AssignOrderNumber)
            .PermitReentry(OrderCommand.RequestClientDocument)
            .PermitReentry(OrderCommand.ProvideRequestedDocument)
            .Permit(OrderCommand.AssignVendor, OrderStatus.VendorAssigned);
        StateMachine.Configure(OrderStatus.VendorAssigned)
            .PermitReentry(OrderCommand.AssignOrderNumber)
            .PermitReentry(OrderCommand.RequestClientDocument)
            .PermitReentry(OrderCommand.ProvideRequestedDocument)
            .Permit(OrderCommand.ScheduleInspection, OrderStatus.InspectionScheduled);
        StateMachine.Configure(OrderStatus.InspectionScheduled)
            .PermitReentry(OrderCommand.AssignOrderNumber)
            .PermitReentry(OrderCommand.RequestClientDocument)
            .PermitReentry(OrderCommand.ProvideRequestedDocument)
            .Permit(OrderCommand.CompleteInspection, OrderStatus.InspectionCompleted);
        StateMachine.Configure(OrderStatus.InspectionCompleted)
            .PermitReentry(OrderCommand.AssignOrderNumber)
            .PermitReentry(OrderCommand.RequestClientDocument)
            .PermitReentry(OrderCommand.ProvideRequestedDocument)
            .Permit(OrderCommand.DeliverReport, OrderStatus.ReportDelivered);
        StateMachine.Configure(OrderStatus.ReportDelivered)
            .PermitReentry(OrderCommand.AssignOrderNumber)
            .PermitReentry(OrderCommand.RequestClientDocument)
            .PermitReentry(OrderCommand.ProvideRequestedDocument)
            .Permit(OrderCommand.ShipOrder, OrderStatus.Shipped);
        StateMachine.Configure(OrderStatus.Shipped)
            .PermitReentry(OrderCommand.AssignOrderNumber);
    }
}

/// <summary>A request for a document from the client, and how far it is.</summary>
[GenerateSerializer, Immutable]
public sealed record OrderDocumentRequest(
    string RequestedFromUserId,
    string DocumentTypeId,
    int DocumentTypeVersion,
    string DocumentTypeName,
    bool NoticeSent,
    bool Provided);

/// <summary>The order's description of work: its latest version, who approves it, and how far it is.</summary>
/// <param name="NoticeSentVersion">The latest version the client was emailed to approve, or null until the first is.</param>
[GenerateSerializer, Immutable]
public sealed record OrderDescriptionOfWork(
    string Id,
    int Version,
    string ApproverUserId,
    bool Approved,
    int? NoticeSentVersion,
    bool ApprovedNoticeSent);

/// <summary>The vendor the order is assigned to, as a tenant-qualified vendor ID, and whether its users were told.</summary>
[GenerateSerializer, Immutable]
public sealed record OrderVendorAssignment(
    string VendorId,
    bool NoticeSent);

/// <summary>The vendor's completed report and whether the staff member the order is assigned to was told about it.</summary>
[GenerateSerializer, Immutable]
public sealed record OrderReport(
    string DocumentId,
    bool NoticeSent);