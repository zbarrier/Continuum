using System.Security.Claims;

using Continuum.CSharpFunctionalExtensions;
using Continuum.CSharpFunctionalExtensions.Orleans;
using Continuum.Domain.Orleans;

using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Abstractions.ReadModels;

using ManagedCode.Orleans.Identity.Core.Extensions;

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
    private readonly ILogger<OrderGrain> _logger;
    private readonly Func<Exception, Error> _grainCallErrorHandler;

    public OrderGrain(IServiceProvider serviceProvider, ILogger<OrderGrain> logger)
        : base(serviceProvider, "kurrentdb")
    {
        _logger = logger;
        _grainCallErrorHandler = ex => GlobalGrainErrorHandler.Default(ex, _logger);
    }

    private string? GetCurrentUserId()
    {
        return this.GetCurrentUser().FindFirstValue(ClaimTypes.NameIdentifier) ?? this.GetCurrentUser().FindFirstValue("sub") ?? null;
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

    [Id(0)]
    public OrderDraftStarted? Draft { get; set; }

    protected override void ConfigureEventHandlers()
    {
        On<OrderDraftStarted>(@event =>
        {
            Status = OrderStatus.Draft;
            Draft = @event;
        });
    }

    protected override void ConfigureStateMachine()
    {
        StateMachine.Configure(OrderStatus.None).Permit(OrderCommand.StartDraft, OrderStatus.Draft);
    }
}