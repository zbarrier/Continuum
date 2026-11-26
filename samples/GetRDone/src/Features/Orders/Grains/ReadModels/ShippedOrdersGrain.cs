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
///     Projects the staff dashboard's Shipped list from the tenant's orders stream (see <see cref="OrderProjections"/>).
/// </summary>
/// <remarks>
///     It works like the <see cref="InProgressOrdersGrain"/>: one instance per tenant, keyed <c>{tenantId}|default</c>.
///     <see cref="AuthorizeAttribute"/> on the class covers the query and <see cref="IProjectionGrain.OnNextBatchAsync"/>,
///     which a catch-up subscription calls signed in as its automation account; stream deliveries arrive through Orleans'
///     stream consumer extension, which it does not cover.
/// </remarks>
[ImplicitStreamSubscription(OrderProjections.TenantOrdersNamespace)]
[GrainType("shippedOrders")]
[Authorize]
public sealed class ShippedOrdersGrain(
    [PersistentState("shippedOrders", OrderProjections.StorageName)] IPersistentState<ShippedOrdersState> storage,
    ILogger<ShippedOrdersGrain> logger)
    : ProjectionGrain<ShippedOrdersGrain, ShippedOrdersState>(storage, logger), IShippedOrdersGrain
{
    public async Task<Result<ShippedOrdersPage>> GetOrdersAsync(int skip, int take)
    {
        return await this.EnsureStaff()
            .Bind(() => StaffReadModels.EnsureValidPage(skip, take))
            .BindTry(async () =>
            {
                TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);
                var names = await UserDirectoryNames.LoadAsync(GrainFactory, tenantId);

                var orders = State.Orders.Values
                    .OrderByDescending(order => order.ShippedAt)
                    .ThenBy(order => order.OrderId, StringComparer.Ordinal)
                    .Skip(skip)
                    .Take(take)
                    .Select(order => new ShippedOrder(order.OrderId, order.OrderNumber, names.ClientName(order.ClientId),
                        order.PropertyAddress, order.ShippedAt))
                    .ToArray();
                return Result.Success(new ShippedOrdersPage(orders, State.Orders.Count));
            }, ex => GlobalGrainErrorHandler.Default(ex, logger));
    }
}

[GenerateSerializer]
public sealed class ShippedOrdersState : HandlerProjectionState<ShippedOrdersState>
{
    public ShippedOrdersState()
    {
        On<OrderDraftStarted>(Apply);
        On<RequestAndPropertySaved>(Apply);
        On<OrderNumberAssigned>(Apply);
        On<OrderShipped>(Apply);
    }

    /// <summary>
    ///     What the list shows of each order that is not shipped yet, by order ID; <see cref="OrderShipped"/>, which puts the
    ///     order on the list, carries little of it. Orders that are never shipped stay here.
    /// </summary>
    [Id(0)] public Dictionary<string, TenantOrderDraftDetails> Pending { get; init; } = [];

    /// <summary>The orders on the list, by order ID.</summary>
    [Id(1)] public Dictionary<string, ShippedOrderEntry> Orders { get; init; } = [];

    private bool Apply(OrderDraftStarted evt, IStreamedEvent<object> streamedEvent)
    {
        Pending[evt.OrderId] = new TenantOrderDraftDetails(evt.ClientId,
            PropertyAddressLine.Format(evt.PropertyStreet, evt.PropertyCity, evt.PropertyState, evt.PropertyZip),
            evt.FormTypeName);
        return true;
    }

    private bool Apply(RequestAndPropertySaved evt, IStreamedEvent<object> streamedEvent)
    {
        if (!Pending.TryGetValue(evt.OrderId, out var pending))
        {
            return false;
        }

        Pending[evt.OrderId] = pending with
        {
            PropertyAddress = PropertyAddressLine.Format(evt.PropertyStreet, evt.PropertyCity, evt.PropertyState, evt.PropertyZip),
            FormTypeName = evt.FormTypeName
        };
        return true;
    }

    /// <summary>Records the number of an order that is not shipped yet, or updates the order on the list.</summary>
    private bool Apply(OrderNumberAssigned evt, IStreamedEvent<object> streamedEvent)
    {
        if (Orders.TryGetValue(evt.OrderId, out var order))
        {
            Orders[evt.OrderId] = order with { OrderNumber = evt.OrderNumber };
            return true;
        }
        if (Pending.TryGetValue(evt.OrderId, out var pending))
        {
            Pending[evt.OrderId] = pending with { OrderNumber = evt.OrderNumber };
            return true;
        }
        return false;
    }

    private bool Apply(OrderShipped evt, IStreamedEvent<object> streamedEvent)
    {
        if (!Pending.Remove(evt.OrderId, out var pending))
        {
            return false;
        }

        Orders[evt.OrderId] = new ShippedOrderEntry(evt.OrderId, evt.OrderNumber ?? pending.OrderNumber, pending.ClientId,
            pending.PropertyAddress, evt.OccurredAt);
        return true;
    }
}

/// <summary>An order on the Shipped list, with its client ID, which the query turns into the client's name.</summary>
[GenerateSerializer, Immutable]
public sealed record ShippedOrderEntry(
    string OrderId,
    long? OrderNumber,
    string ClientId,
    string PropertyAddress,
    DateTimeOffset ShippedAt);
