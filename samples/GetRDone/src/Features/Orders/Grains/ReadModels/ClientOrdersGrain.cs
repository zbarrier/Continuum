using System.Net;

using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Tenancy;
using Continuum.Streaming;
using Continuum.Streaming.Orleans;

using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Abstractions.ReadModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace GetRDone.Orders.Grains.ReadModels;

/// <summary>
///     Projects the orders a client user submitted from the user's client orders stream (see <see cref="OrderProjections"/>).
/// </summary>
/// <remarks>
///     The stream provider activates the grain whose key is the stream key, the user's tenant user ID, so there is one
///     instance per user. Only the query is authorized: stream delivery carries no signed-in user.
/// </remarks>
[ImplicitStreamSubscription(OrderProjections.ClientUserOrdersNamespace)]
[GrainType("clientOrders")]
public sealed class ClientOrdersGrain(
    [PersistentState("clientOrders", OrderProjections.StorageName)] IPersistentState<ClientOrdersState> storage,
    ILogger<ClientOrdersGrain> logger)
    : ProjectionGrain<ClientOrdersGrain, ClientOrdersState>(storage, logger), IClientOrdersGrain
{
    [Authorize]
    public Task<Result<ClientOrdersPage>> GetOrdersAsync(int skip, int take)
    {
        var tenantUserId = this.GetPrimaryKeyString();
        TenantKey.Split(tenantUserId, out var tenantId, out _);
        if (this.GetCurrentTenantUserId(tenantId) != tenantUserId)
        {
            return Task.FromResult(Result.Failure<ClientOrdersPage>(RequestErrors.New(HttpStatusCode.Forbidden,
                Grpc.Core.StatusCode.PermissionDenied, "You can only see your own orders.")));
        }
        if (skip < 0 || take < 1)
        {
            return Task.FromResult(Result.Failure<ClientOrdersPage>(RequestErrors.NewInvalidArg(
                "Skip must not be negative and take must be at least one.")));
        }

        var orders = State.Orders.Values
            .OrderByDescending(order => order.SubmittedAt)
            .ThenBy(order => order.OrderId, StringComparer.Ordinal)
            .Skip(skip)
            .Take(take)
            .ToArray();
        return Task.FromResult(Result.Success(new ClientOrdersPage(orders, State.Orders.Count)));
    }
}

[GenerateSerializer]
public sealed class ClientOrdersState : HandlerProjectionState<ClientOrdersState>
{
    public ClientOrdersState()
    {
        On<OrderDraftStarted>(Apply);
        On<RequestAndPropertySaved>(Apply);
        On<OrderSubmitted>(Apply);
    }

    /// <summary>
    ///     What the list shows of each draft the user has not submitted yet, by order ID; <see cref="OrderSubmitted"/>
    ///     carries none of it.
    /// </summary>
    [Id(0)] public Dictionary<string, ClientOrderDraftDetails> Drafts { get; init; } = [];

    /// <summary>The submitted orders by order ID.</summary>
    [Id(1)] public Dictionary<string, ClientOrder> Orders { get; init; } = [];

    private bool Apply(OrderDraftStarted evt, IStreamedEvent<object> streamedEvent)
    {
        Drafts[evt.OrderId] = new ClientOrderDraftDetails(
            PropertyAddressLine.Format(evt.PropertyStreet, evt.PropertyCity, evt.PropertyState, evt.PropertyZip),
            evt.FormTypeName);
        return true;
    }

    private bool Apply(RequestAndPropertySaved evt, IStreamedEvent<object> streamedEvent)
    {
        if (!Drafts.ContainsKey(evt.OrderId))
        {
            return false;
        }

        Drafts[evt.OrderId] = new ClientOrderDraftDetails(
            PropertyAddressLine.Format(evt.PropertyStreet, evt.PropertyCity, evt.PropertyState, evt.PropertyZip),
            evt.FormTypeName);
        return true;
    }

    private bool Apply(OrderSubmitted evt, IStreamedEvent<object> streamedEvent)
    {
        if (!Drafts.Remove(evt.OrderId, out var draft))
        {
            return false;
        }

        Orders[evt.OrderId] = new ClientOrder(
            evt.OrderId, null, draft.PropertyAddress, draft.FormTypeName, ClientOrderStatus.Submitted, evt.OccurredAt);
        return true;
    }
}

/// <summary>What My orders shows of a draft once it is submitted.</summary>
/// <param name="PropertyAddress">The property address on one line.</param>
/// <param name="FormTypeName">The form type ordered.</param>
[GenerateSerializer, Immutable]
public sealed record ClientOrderDraftDetails(string PropertyAddress, string FormTypeName);
