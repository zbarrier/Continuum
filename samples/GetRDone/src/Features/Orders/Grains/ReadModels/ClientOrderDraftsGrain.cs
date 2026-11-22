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
///     Projects the drafts a client user started from the user's client orders stream (see <see cref="OrderProjections"/>).
/// </summary>
/// <remarks>
///     The stream provider activates the grain whose key is the stream key, the user's tenant user ID, so there is one
///     instance per user. Only the query is authorized: stream delivery carries no signed-in user.
/// </remarks>
[ImplicitStreamSubscription(OrderProjections.ClientUserOrdersNamespace)]
[GrainType("clientOrderDrafts")]
public sealed class ClientOrderDraftsGrain(
    [PersistentState("clientOrderDrafts", OrderProjections.StorageName)] IPersistentState<ClientOrderDraftsState> storage,
    ILogger<ClientOrderDraftsGrain> logger)
    : ProjectionGrain<ClientOrderDraftsGrain, ClientOrderDraftsState>(storage, logger), IClientOrderDraftsGrain
{
    [Authorize]
    public Task<Result<ClientOrderDraftsPage>> GetDraftsAsync(int skip, int take)
    {
        var tenantUserId = this.GetPrimaryKeyString();
        TenantKey.Split(tenantUserId, out var tenantId, out _);
        if (this.GetCurrentTenantUserId(tenantId) != tenantUserId)
        {
            return Task.FromResult(Result.Failure<ClientOrderDraftsPage>(RequestErrors.New(HttpStatusCode.Forbidden,
                Grpc.Core.StatusCode.PermissionDenied, "You can only see your own drafts.")));
        }
        if (skip < 0 || take < 1)
        {
            return Task.FromResult(Result.Failure<ClientOrderDraftsPage>(RequestErrors.NewInvalidArg(
                "Skip must not be negative and take must be at least one.")));
        }

        var drafts = State.Drafts.Values
            .OrderByDescending(draft => draft.LastSavedAt)
            .ThenBy(draft => draft.OrderId, StringComparer.Ordinal)
            .Skip(skip)
            .Take(take)
            .ToArray();
        return Task.FromResult(Result.Success(new ClientOrderDraftsPage(drafts, State.Drafts.Count)));
    }
}

[GenerateSerializer]
public sealed class ClientOrderDraftsState : HandlerProjectionState<ClientOrderDraftsState>
{
    public ClientOrderDraftsState()
    {
        On<OrderDraftStarted>(Apply);
    }

    /// <summary>The drafts by order ID.</summary>
    [Id(0)] public Dictionary<string, ClientOrderDraft> Drafts { get; init; } = [];

    private bool Apply(OrderDraftStarted evt, IStreamedEvent<object> streamedEvent)
    {
        Drafts[evt.OrderId] = new ClientOrderDraft(
            evt.OrderId,
            $"{evt.PropertyStreet}, {evt.PropertyCity}, {evt.PropertyState} {evt.PropertyZip}",
            evt.FormTypeName,
            evt.OccurredAt);
        return true;
    }
}
