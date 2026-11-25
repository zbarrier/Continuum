using Continuum.CSharpFunctionalExtensions;
using Continuum.CSharpFunctionalExtensions.Orleans;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Events;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderNumbers;
using GetRDone.Orders.Grains.OrderAggregate;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

using Orleans.Providers;

namespace GetRDone.Orders.Grains.OrderNumbers;

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
    ///     How many of its most recent events the generator loads and keeps the orders of. A tenant's stream grows with
    ///     every batch issued, so activation reads only these events; an order asked for again after this many later
    ///     batches would be issued a new number, which the order ignores because it keeps the number it was assigned.
    /// </summary>
    public const int RecentEventCount = 100;

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
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId, nameof(OrderNumbersIssued.InitiatedBy)).Deconstruct(out var initiatedBy),
                    Result.CombineForValidation(requested.Select((orderId, i) => (Result)OrderId.Create(orderId, tenantId, $"OrderIds[{i}]"))))
                .Bind(() => _permissions.CanIssueOrderNumbers(initiatedBy!))
                .BindWithStreamTransaction(this, transaction =>
                {
                    var lastNumber = Math.Max(State.LastOrderNumber, FirstOrderNumber - 1);
                    var numbers = new List<IssuedOrderNumber>();
                    foreach (var orderId in requested.Where(orderId => !State.OrderNumbers.ContainsKey(orderId)))
                    {
                        numbers.Add(new IssuedOrderNumber { OrderId = orderId, OrderNumber = ++lastNumber });
                    }
                    if (numbers.Count == 0)
                    {
                        return;
                    }
                    transaction.Apply(new OrderNumbersIssued
                    {
                        Numbers = [.. numbers],
                        InitiatedBy = initiatedBy!,
                        OccurredAt = DateTimeOffset.UtcNow
                    });
                })
                .Map(response => new IssueOrderNumbersResponse(response.Changes, response.Version,
                    requested.ToDictionary(orderId => orderId, orderId => State.OrderNumbers[orderId], StringComparer.Ordinal)));
        }, _grainCallErrorHandler);
    }
}

[GenerateSerializer]
public sealed class OrderNumberGeneratorState : EventDrivenState<OrderNumberGeneratorState, OrderNumberGeneratorEvent>
{
    public OrderNumberGeneratorState()
    {
        On<OrderNumbersIssued>(Handle);
    }

    /// <summary>The highest number issued, or 0 before the first.</summary>
    [Id(0)] public long LastOrderNumber { get; private set; }

    /// <summary>
    ///     The number issued to each order of the generator's most recent events, by order ID, so an order asked for again
    ///     is given the number it already has.
    /// </summary>
    [Id(1)] public Dictionary<string, long> OrderNumbers { get; private set; } = [];

    /// <summary>
    ///     The order IDs of each of the generator's most recent events, oldest first. Only the last
    ///     <see cref="OrderNumberGeneratorGrain.RecentEventCount" /> are kept, matching the events an activation loads, so a
    ///     long-lived activation keeps the same orders as a new one.
    /// </summary>
    [Id(2)] public Queue<string[]> RecentOrderIds { get; private set; } = new();

    private void Handle(OrderNumbersIssued evt)
    {
        foreach (var issued in evt.Numbers)
        {
            OrderNumbers[issued.OrderId] = issued.OrderNumber;
            LastOrderNumber = Math.Max(LastOrderNumber, issued.OrderNumber);
        }
        RecentOrderIds.Enqueue([.. evt.Numbers.Select(issued => issued.OrderId)]);
        while (RecentOrderIds.Count > OrderNumberGeneratorGrain.RecentEventCount)
        {
            foreach (var orderId in RecentOrderIds.Dequeue())
            {
                OrderNumbers.Remove(orderId);
            }
        }
    }
}
