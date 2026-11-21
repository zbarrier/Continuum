using System.Security.Claims;

using Continuum.CSharpFunctionalExtensions;
using Continuum.CSharpFunctionalExtensions.Orleans;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Tenancy;

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
    ///     Gets the current user's ID qualified by <paramref name="tenantId"/>, as <c>{TenantId}|{UserGuid}</c>. The identity
    ///     provider only knows the user GUID; a full application would also check the user's access to the tenant.
    /// </summary>
    private string? GetCurrentUserId(string? tenantId)
    {
        var user = this.GetCurrentUser();
        var userGuid = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userGuid))
        {
            return null;
        }
        return tenantId is null ? userGuid : $"{tenantId}{TenantKey.Separator}{userGuid}";
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