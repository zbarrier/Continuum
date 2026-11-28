using Continuum.CSharpFunctionalExtensions;
using Continuum.CSharpFunctionalExtensions.Orleans;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Events;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Abstractions.OrderNumberGeneratorAggregate;
using GetRDone.Orders.Grains.OrderAggregate;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

using Orleans.Providers;

namespace GetRDone.Orders.Grains.OrderNumberGeneratorAggregate;

[Authorize]
[GrainType("orderNumberGenerator")]
[LogConsistencyProvider(ProviderName = "EventLog")]
[LoadMostRecentEventsOnly(RecentEventCount)]
public sealed class OrderNumberGeneratorGrain
    : EventDrivenGrain<OrderNumberGeneratorState, OrderNumberGeneratorEvent>, IOrderNumberGeneratorGrain
{
    /// <summary>The number a tenant's first order gets; the sample starts where the demo's numbers start.</summary>
    public const long FirstOrderNumber = 610_696;

    /// <summary>
    ///     How many of its most recent events the generator loads, which is also how many of its most recent orders it
    ///     remembers, since each event issues one order its number. A tenant's stream grows with every order, so
    ///     activation reads only these events; an order asked for again after this many later orders would be issued a
    ///     new number, which the order ignores because it keeps the number it was assigned.
    /// </summary>
    public const int RecentEventCount = 1000;

    private readonly IOrderPermissions _permissions;
    private readonly Func<Exception, Error> _grainCallErrorHandler;

    public OrderNumberGeneratorGrain(IServiceProvider serviceProvider, IOrderPermissions permissions, ILogger<OrderNumberGeneratorGrain> logger)
        : base(serviceProvider, "kurrentdb")
    {
        _permissions = permissions;
        _grainCallErrorHandler = ex => GlobalGrainErrorHandler.Default(ex, logger);
    }

    public async Task<Result<IssueOrderNumbersResponse>> IssueOrderNumbersAsync(string[] orderIds)
    {
        return await Result.Success().BindTry(async () =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);
            var requested = (orderIds ?? []).Distinct(StringComparer.Ordinal).ToArray();

            return await Result.CombineForValidation(
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId, nameof(OrderNumberIssued.InitiatedBy)).Deconstruct(out var initiatedBy),
                    Result.CombineForValidation(requested.Select((orderId, i) => (Result)OrderId.Create(orderId, tenantId, $"OrderIds[{i}]"))))
                .Bind(() => _permissions.CanIssueOrderNumbers(initiatedBy!))
                .BindWithStreamTransaction(this, transaction =>
                {
                    var lastNumber = Math.Max(State.LastOrderNumber, FirstOrderNumber - 1);
                    var occurredAt = DateTimeOffset.UtcNow;
                    foreach (var orderId in requested.Where(orderId => !State.OrderNumbers.ContainsKey(orderId)))
                    {
                        transaction.Apply(new OrderNumberIssued
                        {
                            OrderId = orderId,
                            OrderNumber = ++lastNumber,
                            InitiatedBy = initiatedBy!,
                            OccurredAt = occurredAt
                        });
                    }
                })
                .Map(response => new IssueOrderNumbersResponse(response.Changes, response.Version,
                    requested.ToDictionary(orderId => orderId, orderId => State.OrderNumbers[orderId], StringComparer.Ordinal)));
        }, _grainCallErrorHandler);
    }

    /// <summary>
    ///     Sends each issued number to its order's stream as well, alone rather than with the rest of the batch, so an
    ///     order can react to its own number.
    /// </summary>
    protected override IReadOnlyList<StreamTarget> GetStreamTargets(object @event)
        => @event is OrderNumberIssued issued ? [StreamTarget.Event(GrainFactory.GetGrain<IOrderGrain>(issued.OrderId).GetGrainId())] : [];
}

[GenerateSerializer]
public sealed class OrderNumberGeneratorState : EventDrivenState<OrderNumberGeneratorState, OrderNumberGeneratorEvent>
{
    public OrderNumberGeneratorState()
    {
        On<OrderNumberIssued>(Handle);
    }

    /// <summary>The highest number issued, or 0 before the first.</summary>
    [Id(0)] public long LastOrderNumber { get; private set; }

    /// <summary>
    ///     The number issued to each of the generator's most recent orders, by order ID, so an order asked for again is
    ///     given the number it already has.
    /// </summary>
    [Id(1)] public Dictionary<string, long> OrderNumbers { get; private set; } = [];

    /// <summary>
    ///     The generator's most recent orders, oldest first. Only the last
    ///     <see cref="OrderNumberGeneratorGrain.RecentEventCount" /> are kept, matching the events an activation loads, so a
    ///     long-lived activation keeps the same orders as a new one.
    /// </summary>
    [Id(2)] public Queue<string> RecentOrderIds { get; private set; } = new();

    private void Handle(OrderNumberIssued evt)
    {
        OrderNumbers[evt.OrderId] = evt.OrderNumber;
        LastOrderNumber = Math.Max(LastOrderNumber, evt.OrderNumber);
        RecentOrderIds.Enqueue(evt.OrderId);
        while (RecentOrderIds.Count > OrderNumberGeneratorGrain.RecentEventCount)
        {
            OrderNumbers.Remove(RecentOrderIds.Dequeue());
        }
    }
}
