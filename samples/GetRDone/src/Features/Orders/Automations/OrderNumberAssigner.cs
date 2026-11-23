using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Abstractions.OrderNumbers;

using Microsoft.Extensions.Logging;

namespace GetRDone.Orders.Automations;

/// <summary>
///     The second step of giving submitted orders their numbers: gives each order the number the
///     <see cref="OrderNumberIssuer"/> had its tenant's generator issue it, calling the orders in parallel.
/// </summary>
/// <remarks>
///     Runs as <see cref="AutomationUser.OrderNumberAssigner"/>. An order that already has its number succeeds without
///     changes, and one that can't be given its number (a business error) is logged and skipped. When calls fail in a
///     way worth retrying, the orders already handled are remembered so the retried batch calls only those still
///     waiting. Batches are handled one at a time, so the instance is not thread-safe.
/// </remarks>
public sealed partial class OrderNumberAssigner(IGrainFactory grainFactory, ILogger<OrderNumberAssigner> logger)
{
    private readonly HashSet<string> _handledOrderIds = new(StringComparer.Ordinal);

    /// <summary>Gives the orders their <paramref name="issuedNumbers"/>.</summary>
    /// <exception cref="OrderNumbersPendingException">Some orders were not given their numbers and are worth retrying.</exception>
    public async Task AssignAsync(IEnumerable<IssuedOrderNumber> issuedNumbers)
    {
        var tenants = issuedNumbers
            .DistinctBy(issued => issued.OrderId, StringComparer.Ordinal)
            .Where(issued => !_handledOrderIds.Contains(issued.OrderId))
            .GroupBy(issued => TenantOf(issued.OrderId), StringComparer.Ordinal);
        var outcomes = (await Task.WhenAll(tenants.Select(tenant => AssignTenantAsync(tenant.Key, [.. tenant])))).SelectMany(outcome => outcome);

        var pending = 0;
        foreach (var (orderId, isHandled) in outcomes)
        {
            if (isHandled)
            {
                _handledOrderIds.Add(orderId);
            }
            else
            {
                pending++;
            }
        }
        if (pending > 0)
        {
            throw new OrderNumbersPendingException($"{pending} orders are still waiting to be given their order numbers.");
        }
        _handledOrderIds.Clear();
    }

    /// <summary>Gives one tenant's orders their numbers, signed in to the tenant.</summary>
    private async Task<(string OrderId, bool IsHandled)[]> AssignTenantAsync(string tenantId, IssuedOrderNumber[] issuedNumbers)
    {
        AutomationUser.OrderNumberAssigner.SignIn(tenantId);
        return await Task.WhenAll(issuedNumbers.Select(AssignOrderAsync));
    }

    /// <returns>Whether the order is handled; <see langword="false"/> when the call is worth retrying.</returns>
    private async Task<(string OrderId, bool IsHandled)> AssignOrderAsync(IssuedOrderNumber issued)
    {
        try
        {
            var result = await grainFactory.GetGrain<IOrderGrain>(issued.OrderId).AssignOrderNumberAsync(issued.OrderNumber);
            if (result.IsSuccess)
            {
                return (issued.OrderId, true);
            }
            if (OrderNumbersPendingException.IsRetryable(result.Error))
            {
                LogAssignFailedRetrying(issued.OrderId, issued.OrderNumber, result.Error);
                return (issued.OrderId, false);
            }
            LogAssignFailedSkipped(issued.OrderId, issued.OrderNumber, result.Error);
            return (issued.OrderId, true);
        }
        catch (Exception exception)
        {
            LogAssignThrewRetrying(exception, issued.OrderId, issued.OrderNumber);
            return (issued.OrderId, false);
        }
    }

    private static string TenantOf(string orderId)
    {
        TenantKey.Split(orderId, out var tenantId, out _);
        return tenantId!;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Giving order {OrderId} order number {OrderNumber} failed and will be retried: {Error}")]
    private partial void LogAssignFailedRetrying(string orderId, long orderNumber, Error error);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Giving order {OrderId} order number {OrderNumber} failed and will be retried.")]
    private partial void LogAssignThrewRetrying(Exception exception, string orderId, long orderNumber);

    [LoggerMessage(Level = LogLevel.Error, Message = "Order {OrderId} was not given order number {OrderNumber} and is skipped: {Error}")]
    private partial void LogAssignFailedSkipped(string orderId, long orderNumber, Error error);
}
