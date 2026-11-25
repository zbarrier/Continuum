using Continuum.CSharpFunctionalExtensions;
using Continuum.CSharpFunctionalExtensions.Orleans;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Events;

using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Abstractions.ReadModels;
using GetRDone.Orders.Grains.ReadModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

using Orleans.Providers;

namespace GetRDone.Orders.Grains.OrderAggregate;

[Authorize]
[GrainType("order")]
[LogConsistencyProvider(ProviderName = "EventLog")]
public sealed partial class OrderGrain
    : EventDrivenStateMachineGrain<OrderState, OrderEvent, OrderStatus, OrderCommand>, IOrderGrain
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
    ///     Sends every order event to the stream of the user who started the order as well, so the client user's read
    ///     models (such as <see cref="ClientOrderDraftsGrain"/> and <see cref="ClientOrdersGrain"/>) see all of that user's
    ///     orders on one stream.
    /// </summary>
    protected override IReadOnlyList<StreamTarget> GetStreamTargets(object @event)
    {
        // The draft started event is what records the starter, so it is not in the state yet while it is appended.
        var startedBy = @event is OrderDraftStarted started ? started.InitiatedBy : State.StartedBy;
        return startedBy is null ? [] : [StreamTarget.Transaction(OrderProjections.ClientUserOrdersStreamTarget(startedBy))];
    }
}

public enum OrderStatus
{
    None,
    Draft,
    Submitted
}

public enum OrderCommand
{
    StartDraft,
    SaveRequestAndProperty,
    SaveBorrowerAndAccessContact,
    AttachSupportingDocument,
    RemoveSupportingDocument,
    SubmitOrder,
    AssignOrderNumber
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

    /// <summary>The order's number, or null until the Order Number Assigner gives the submitted order its number.</summary>
    [Id(4)] public long? OrderNumber { get; private set; }

    protected override void ConfigureEventHandlers()
    {
        On<OrderDraftStarted>(Handle);
        On<BorrowerAndAccessContactSaved>(Handle);
        On<SupportingDocumentAttached>(Handle);
        On<SupportingDocumentRemoved>(Handle);
        On<OrderSubmitted>(Handle);
        On<OrderNumberAssigned>(Handle);
    }

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
            .PermitReentry(OrderCommand.AssignOrderNumber);
    }
}