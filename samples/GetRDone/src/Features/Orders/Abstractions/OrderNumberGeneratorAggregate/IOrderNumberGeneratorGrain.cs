using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Tenancy;
using Continuum.TypeMapping;

using GetRDone.Orders.Abstractions.ReadModels;

namespace GetRDone.Orders.Abstractions.OrderNumberGeneratorAggregate;

/// <summary>
///     Issues a tenant's order numbers, one after another, so no two orders of the tenant get the same number. Each
///     tenant has one generator, keyed <c>{TenantId}|default</c>. The Order Number Issuer automation asks it for the
///     numbers of submitted orders, and the Order Number Assigner then gives each order its number.
/// </summary>
public interface IOrderNumberGeneratorGrain : IEventDrivenGrain
{
    /// <summary>
    ///     Issues the next numbers to the orders that don't have one yet, recording one <see cref="OrderNumberIssued"/>
    ///     per order in the order they were asked for, and returns the number of every order asked for, including those
    ///     issued earlier. Asking again for the same orders issues nothing, so the automation can safely retry. Only the
    ///     Order Number Issuer automation may issue numbers.
    /// </summary>
    Task<Result<IssueOrderNumbersResponse>> IssueOrderNumbersAsync(string[] orderIds);
}

/// <param name="orderNumbers">The number of each order asked for, by order ID.</param>
[Alias("Orders.IssueOrderNumbersResponse.V1"), GenerateSerializer, Immutable]
public sealed class IssueOrderNumbersResponse(IReadOnlyList<IChange> changes, int version, IReadOnlyDictionary<string, long> orderNumbers)
    : CommandResponseWithChangesBase(changes, version)
{
    /// <summary>The number of each order asked for, by order ID.</summary>
    [Id(0)] public IReadOnlyDictionary<string, long> OrderNumbers { get; } = orderNumbers;
}

[GenerateSerializer, Immutable]
public abstract class OrderNumberGeneratorEvent : DomainEvent;

/// <summary>
///     An order number was issued to a submitted order. A batch of orders is issued in one transaction, one event per
///     order, and each event also goes to its order's stream alone.
/// </summary>
[Alias("Orders.OrderNumberIssued.V1"), DomainEventType("Orders.OrderNumberIssued.V1"), GenerateSerializer, Immutable]
public sealed class OrderNumberIssued : OrderNumberGeneratorEvent
{
    [Id(0)] public required string OrderId { get; init; }
    [Id(1)] public required long OrderNumber { get; init; }
}

public static class OrderNumberGeneratorGrainFactoryExtensions
{
    /// <summary>Gets the order number generator of <paramref name="tenantId"/>.</summary>
    public static IOrderNumberGeneratorGrain GetOrderNumberGeneratorGrain(this IGrainFactory grainFactory, string? tenantId)
        => grainFactory.GetGrain<IOrderNumberGeneratorGrain>(TenantKey.Create(tenantId, ReadModelGrainFactoryExtensions.DefaultKey));
}
