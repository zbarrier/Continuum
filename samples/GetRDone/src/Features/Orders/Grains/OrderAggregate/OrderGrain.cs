using Continuum.CSharpFunctionalExtensions;
using Continuum.CSharpFunctionalExtensions.Orleans;
using Continuum.Domain.Orleans;

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
    ///     models (such as <see cref="ClientOrderDraftsGrain"/>) see all of that user's orders on one stream.
    /// </summary>
    protected override IReadOnlyList<GrainId> GetStreamTargets(object @event)
    {
        // The draft started event is what records the starter, so it is not in the state yet while it is appended.
        var startedBy = @event is OrderDraftStarted started ? started.InitiatedBy : State.StartedBy;
        return startedBy is null ? [] : [OrderProjections.ClientUserOrdersStreamTarget(startedBy)];
    }
}

public enum OrderStatus
{
    None,
    Draft
}

public enum OrderCommand
{
    StartDraft
}

[GenerateSerializer]
public sealed partial class OrderState : EventDrivenStateMachineState<OrderState, OrderEvent, OrderStatus, OrderCommand>
{
    public OrderState() : base(OrderStatus.None) { }

    /// <summary>The user who started the order, as a tenant-qualified user ID.</summary>
    [Id(0)] public string? StartedBy { get; private set; }

    protected override void ConfigureEventHandlers()
    {
        On<OrderDraftStarted>(Handle);
    }

    protected override void ConfigureStateMachine()
    {
        StateMachine.Configure(OrderStatus.None).Permit(OrderCommand.StartDraft, OrderStatus.Draft);
    }
}