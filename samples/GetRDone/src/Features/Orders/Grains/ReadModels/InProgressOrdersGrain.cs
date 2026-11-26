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
///     Projects the staff dashboard's In progress list from the tenant's orders stream (see <see cref="OrderProjections"/>).
/// </summary>
/// <remarks>
///     The stream provider activates the grain whose key is the stream key, <c>{tenantId}|default</c>, so there is one
///     instance per tenant. <see cref="AuthorizeAttribute"/> on the class covers the query and
///     <see cref="IProjectionGrain.OnNextBatchAsync"/>, which a catch-up subscription calls signed in as its automation
///     account; stream deliveries arrive through Orleans' stream consumer extension, which it does not cover.
/// </remarks>
[ImplicitStreamSubscription(OrderProjections.TenantOrdersNamespace)]
[GrainType("inProgressOrders")]
[Authorize]
public sealed class InProgressOrdersGrain(
    [PersistentState("inProgressOrders", OrderProjections.StorageName)] IPersistentState<InProgressOrdersState> storage,
    ILogger<InProgressOrdersGrain> logger)
    : ProjectionGrain<InProgressOrdersGrain, InProgressOrdersState>(storage, logger), IInProgressOrdersGrain
{
    public async Task<Result<InProgressOrdersPage>> GetOrdersAsync(int skip, int take)
    {
        return await this.EnsureStaff()
            .Bind(() => StaffReadModels.EnsureValidPage(skip, take))
            .BindTry(async () =>
            {
                TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);
                var names = await UserDirectoryNames.LoadAsync(GrainFactory, tenantId);

                var orders = State.Orders.Values
                    .OrderBy(order => order.DueDate)
                    .ThenBy(order => order.OrderNumber ?? long.MaxValue)
                    .ThenBy(order => order.OrderId, StringComparer.Ordinal)
                    .Skip(skip)
                    .Take(take)
                    .Select(order => new InProgressOrder(order.OrderId, order.OrderNumber, names.ClientName(order.ClientId),
                        order.PropertyAddress, order.AssignedToUserId, names.UserName(order.AssignedToUserId), order.Status,
                        order.DueDate))
                    .ToArray();
                return Result.Success(new InProgressOrdersPage(orders, State.Orders.Count));
            }, ex => GlobalGrainErrorHandler.Default(ex, logger));
    }
}

[GenerateSerializer]
public sealed class InProgressOrdersState : HandlerProjectionState<InProgressOrdersState>
{
    public InProgressOrdersState()
    {
        On<OrderDraftStarted>(Apply);
        On<RequestAndPropertySaved>(Apply);
        On<OrderNumberAssigned>(Apply);
        On<OrderAssigned>(Apply);
        On<OrderAcknowledged>(Apply);
        On<DescriptionOfWorkSubmitted>(Apply);
        On<DescriptionOfWorkApproved>(Apply);
        On<VendorAssigned>(Apply);
    }

    /// <summary>
    ///     What the list shows of each order that is not assigned yet, by order ID; <see cref="OrderAssigned"/>, which puts
    ///     the order on the list, carries none of it. Drafts the client never submits stay here.
    /// </summary>
    [Id(0)] public Dictionary<string, TenantOrderDraftDetails> Drafts { get; init; } = [];

    /// <summary>The orders on the list, by order ID.</summary>
    [Id(1)] public Dictionary<string, InProgressOrderEntry> Orders { get; init; } = [];

    private bool Apply(OrderDraftStarted evt, IStreamedEvent<object> streamedEvent)
    {
        Drafts[evt.OrderId] = new TenantOrderDraftDetails(evt.ClientId,
            PropertyAddressLine.Format(evt.PropertyStreet, evt.PropertyCity, evt.PropertyState, evt.PropertyZip),
            evt.FormTypeName, evt.DueDate);
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
            FormTypeName = evt.FormTypeName,
            DueDate = evt.DueDate
        };
        return true;
    }

    /// <summary>Records the number of an order that is not assigned yet, or updates the order on the list.</summary>
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

    private bool Apply(OrderAssigned evt, IStreamedEvent<object> streamedEvent)
    {
        if (!Drafts.Remove(evt.OrderId, out var draft))
        {
            return false;
        }

        Orders[evt.OrderId] = new InProgressOrderEntry(evt.OrderId, draft.OrderNumber, draft.ClientId, draft.PropertyAddress,
            evt.AssignedToUserId, StaffOrderStatus.Assigned, draft.DueDate);
        return true;
    }

    private bool Apply(OrderAcknowledged evt, IStreamedEvent<object> streamedEvent)
        => SetStatus(evt.OrderId, StaffOrderStatus.Acknowledged);

    private bool Apply(DescriptionOfWorkSubmitted evt, IStreamedEvent<object> streamedEvent)
        => SetStatus(evt.OrderId, StaffOrderStatus.DescriptionOfWorkAwaitingApproval);

    private bool Apply(DescriptionOfWorkApproved evt, IStreamedEvent<object> streamedEvent)
        => SetStatus(evt.OrderId, StaffOrderStatus.DescriptionOfWorkApproved);

    private bool Apply(VendorAssigned evt, IStreamedEvent<object> streamedEvent)
        => SetStatus(evt.OrderId, StaffOrderStatus.VendorAssigned);

    private bool SetStatus(string orderId, StaffOrderStatus status)
    {
        if (!Orders.TryGetValue(orderId, out var order) || order.Status == status)
        {
            return false;
        }

        Orders[orderId] = order with { Status = status };
        return true;
    }
}

/// <summary>An order on the In progress list, with the IDs the query turns into names.</summary>
[GenerateSerializer, Immutable]
public sealed record InProgressOrderEntry(
    string OrderId,
    long? OrderNumber,
    string ClientId,
    string PropertyAddress,
    string AssignedToUserId,
    StaffOrderStatus Status,
    DateOnly DueDate);
