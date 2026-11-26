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
///     Projects the staff dashboard's Unassigned list from the tenant's orders stream (see <see cref="OrderProjections"/>).
/// </summary>
/// <remarks>
///     The stream provider activates the grain whose key is the stream key, <c>{tenantId}|default</c>, so there is one
///     instance per tenant. <see cref="AuthorizeAttribute"/> on the class covers the query and
///     <see cref="IProjectionGrain.OnNextBatchAsync"/>, which a catch-up subscription calls signed in as its automation
///     account; stream deliveries arrive through Orleans' stream consumer extension, which it does not cover.
/// </remarks>
[ImplicitStreamSubscription(OrderProjections.TenantOrdersNamespace)]
[GrainType("unassignedOrders")]
[Authorize]
public sealed class UnassignedOrdersGrain(
    [PersistentState("unassignedOrders", OrderProjections.StorageName)] IPersistentState<UnassignedOrdersState> storage,
    ILogger<UnassignedOrdersGrain> logger)
    : ProjectionGrain<UnassignedOrdersGrain, UnassignedOrdersState>(storage, logger), IUnassignedOrdersGrain
{
    public async Task<Result<UnassignedOrdersPage>> GetOrdersAsync(int skip, int take)
    {
        return await this.EnsureStaff()
            .Bind(() => StaffReadModels.EnsureValidPage(skip, take))
            .BindTry(async () =>
            {
                TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);
                var names = await UserDirectoryNames.LoadAsync(GrainFactory, tenantId);

                var orders = State.Orders.Values
                    .OrderBy(order => order.OrderNumber)
                    .Skip(skip)
                    .Take(take)
                    .Select(order => new UnassignedOrder(order.OrderId, order.OrderNumber, names.ClientName(order.ClientId),
                        order.PropertyAddress, order.FormTypeName))
                    .ToArray();
                return Result.Success(new UnassignedOrdersPage(orders, State.Orders.Count));
            }, ex => GlobalGrainErrorHandler.Default(ex, logger));
    }
}

[GenerateSerializer]
public sealed class UnassignedOrdersState : HandlerProjectionState<UnassignedOrdersState>
{
    public UnassignedOrdersState()
    {
        On<OrderDraftStarted>(Apply);
        On<RequestAndPropertySaved>(Apply);
        On<OrderNumberAssigned>(Apply);
        On<OrderAssigned>(Apply);
    }

    /// <summary>
    ///     What the list shows of each order that has no number yet, by order ID; <see cref="OrderNumberAssigned"/>, which
    ///     puts the order on the list, carries none of it. Drafts the client never submits stay here.
    /// </summary>
    [Id(0)] public Dictionary<string, TenantOrderDraftDetails> Drafts { get; init; } = [];

    /// <summary>The orders on the list, by order ID.</summary>
    [Id(1)] public Dictionary<string, UnassignedOrderEntry> Orders { get; init; } = [];

    private bool Apply(OrderDraftStarted evt, IStreamedEvent<object> streamedEvent)
    {
        Drafts[evt.OrderId] = new TenantOrderDraftDetails(evt.ClientId,
            PropertyAddressLine.Format(evt.PropertyStreet, evt.PropertyCity, evt.PropertyState, evt.PropertyZip),
            evt.FormTypeName);
        return true;
    }

    private bool Apply(RequestAndPropertySaved evt, IStreamedEvent<object> streamedEvent)
    {
        if (!Drafts.TryGetValue(evt.OrderId, out var draft))
        {
            return false;
        }

        Drafts[evt.OrderId] = draft with
        {
            PropertyAddress = PropertyAddressLine.Format(evt.PropertyStreet, evt.PropertyCity, evt.PropertyState, evt.PropertyZip),
            FormTypeName = evt.FormTypeName
        };
        return true;
    }

    private bool Apply(OrderNumberAssigned evt, IStreamedEvent<object> streamedEvent)
    {
        if (!Drafts.Remove(evt.OrderId, out var draft))
        {
            return false;
        }

        Orders[evt.OrderId] = new UnassignedOrderEntry(
            evt.OrderId, evt.OrderNumber, draft.ClientId, draft.PropertyAddress, draft.FormTypeName);
        return true;
    }

    /// <summary>Takes the order off the list, or keeps it off when it is assigned before it has its number.</summary>
    private bool Apply(OrderAssigned evt, IStreamedEvent<object> streamedEvent)
        => Orders.Remove(evt.OrderId) | Drafts.Remove(evt.OrderId);
}

/// <summary>What the staff dashboard shows of an order before it is on one of its lists.</summary>
/// <param name="ClientId">The client the order is for, as a tenant-qualified client ID.</param>
/// <param name="PropertyAddress">The property address on one line.</param>
/// <param name="FormTypeName">The form type ordered.</param>
/// <param name="OrderNumber">The order number, once the order has it.</param>
[GenerateSerializer, Immutable]
public sealed record TenantOrderDraftDetails(
    string ClientId,
    string PropertyAddress,
    string FormTypeName,
    long? OrderNumber = null);

/// <summary>An order on the Unassigned list, with its client ID, which the query turns into the client's name.</summary>
[GenerateSerializer, Immutable]
public sealed record UnassignedOrderEntry(
    string OrderId,
    long OrderNumber,
    string ClientId,
    string PropertyAddress,
    string FormTypeName);
