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
///     Projects the orders the tenant assigned to vendors from the tenant's orders stream (see <see cref="OrderProjections"/>).
/// </summary>
/// <remarks>
///     The stream provider activates the grain whose key is the stream key, <c>{tenantId}|default</c>, so there is one
///     instance per tenant, and the query returns only the signed-in vendor user's vendor's orders.
///     <see cref="AuthorizeAttribute"/> on the class covers the query and <see cref="IProjectionGrain.OnNextBatchAsync"/>,
///     which a catch-up subscription calls signed in as its automation account; stream deliveries arrive through Orleans'
///     stream consumer extension, which it does not cover.
/// </remarks>
[ImplicitStreamSubscription(OrderProjections.TenantOrdersNamespace)]
[GrainType("vendorOrders")]
[Authorize]
public sealed class VendorOrdersGrain(
    [PersistentState("vendorOrders", OrderProjections.StorageName)] IPersistentState<VendorOrdersState> storage,
    ILogger<VendorOrdersGrain> logger)
    : ProjectionGrain<VendorOrdersGrain, VendorOrdersState>(storage, logger), IVendorOrdersGrain
{
    public async Task<Result<VendorOrdersPage>> GetOrdersAsync(int skip, int take)
    {
        TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);
        return await StaffReadModels.EnsureValidPage(skip, take)
            .BindTry(async () =>
            {
                var names = await UserDirectoryNames.LoadAsync(GrainFactory, tenantId);

                return this.GetCurrentVendorId(names, tenantId).Map(vendorId =>
                {
                    var vendorOrders = State.Orders.Values.Where(order => order.VendorId == vendorId).ToArray();
                    var orders = vendorOrders
                        .OrderBy(order => order.VendorDueDate)
                        .ThenBy(order => order.OrderNumber ?? long.MaxValue)
                        .ThenBy(order => order.OrderId, StringComparer.Ordinal)
                        .Skip(skip)
                        .Take(take)
                        .Select(order => new VendorOrder(order.OrderId, order.OrderNumber, order.PropertyAddress,
                            order.FormTypeName, order.Status, order.VendorDueDate))
                        .ToArray();
                    return new VendorOrdersPage(orders, vendorOrders.Length);
                });
            }, ex => GlobalGrainErrorHandler.Default(ex, logger));
    }
}

[GenerateSerializer]
public sealed class VendorOrdersState : HandlerProjectionState<VendorOrdersState>
{
    public VendorOrdersState()
    {
        On<OrderDraftStarted>(Apply);
        On<RequestAndPropertySaved>(Apply);
        On<OrderNumberAssigned>(Apply);
        On<VendorAssigned>(Apply);
    }

    /// <summary>
    ///     What the list shows of each order that is not assigned to a vendor yet, by order ID; <see cref="VendorAssigned"/>,
    ///     which puts the order on the list, carries none of it. Drafts the client never submits stay here.
    /// </summary>
    [Id(0)] public Dictionary<string, TenantOrderDraftDetails> Drafts { get; init; } = [];

    /// <summary>The orders assigned to vendors, by order ID.</summary>
    [Id(1)] public Dictionary<string, VendorOrderEntry> Orders { get; init; } = [];

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

    /// <summary>Records the number of an order that is not assigned to a vendor yet, or updates the order on the list.</summary>
    private bool Apply(OrderNumberAssigned evt, IStreamedEvent<object> streamedEvent)
    {
        if (Orders.TryGetValue(evt.OrderId, out var order))
        {
            Orders[evt.OrderId] = order with { OrderNumber = evt.OrderNumber };
            return true;
        }
        if (Drafts.TryGetValue(evt.OrderId, out var draft))
        {
            Drafts[evt.OrderId] = draft with { OrderNumber = evt.OrderNumber };
            return true;
        }
        return false;
    }

    private bool Apply(VendorAssigned evt, IStreamedEvent<object> streamedEvent)
    {
        if (!Drafts.Remove(evt.OrderId, out var draft))
        {
            return false;
        }

        Orders[evt.OrderId] = new VendorOrderEntry(evt.OrderId, draft.OrderNumber ?? evt.OrderNumber, evt.VendorId,
            draft.PropertyAddress, draft.FormTypeName, VendorOrderStatus.Assigned, evt.VendorDueDate);
        return true;
    }
}

/// <summary>An order on a vendor's Orders list, with the vendor it is assigned to.</summary>
[GenerateSerializer, Immutable]
public sealed record VendorOrderEntry(
    string OrderId,
    long? OrderNumber,
    string VendorId,
    string PropertyAddress,
    string FormTypeName,
    VendorOrderStatus Status,
    DateOnly VendorDueDate);
