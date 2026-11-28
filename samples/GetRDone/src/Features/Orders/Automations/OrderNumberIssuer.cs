using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Abstractions.OrderNumberGeneratorAggregate;

using Microsoft.Extensions.Logging;

namespace GetRDone.Orders.Automations;

/// <summary>
///     The first step of giving submitted orders their numbers: asks each tenant's order number generator to issue
///     numbers to the orders submitted in a batch. Each order then reacts to the number it was issued, which the
///     generator sends to the order's stream, by assigning it to itself.
/// </summary>
/// <remarks>
///     Runs as <see cref="AutomationUser.OrderNumberAssigner"/>. The generator issues an order only one number, so
///     retrying a batch that was partly issued issues nothing twice. Only an order that can never be issued a number is
///     skipped (see <see cref="AutomationErrors.IsRetryable"/>); every other failure is retried.
/// </remarks>
public sealed partial class OrderNumberIssuer(IGrainFactory grainFactory, ILogger<OrderNumberIssuer> logger)
{
    /// <summary>Issues numbers to the <paramref name="submittedOrders"/>.</summary>
    /// <exception cref="OrderNumbersPendingException">Some tenants' numbers were not issued and are worth retrying.</exception>
    public async Task IssueAsync(IEnumerable<OrderSubmitted> submittedOrders)
    {
        var tenants = submittedOrders
            .Select(submitted => submitted.OrderId)
            .Distinct(StringComparer.Ordinal)
            .GroupBy(TenantOf, StringComparer.Ordinal);
        var issued = await Task.WhenAll(tenants.Select(tenant => IssueTenantAsync(tenant.Key, [.. tenant])));

        var pending = issued.Count(isIssued => !isIssued);
        if (pending > 0)
        {
            throw new OrderNumbersPendingException($"The order numbers of {pending} of {issued.Length} tenants are still waiting to be issued.");
        }
    }

    /// <summary>Issues the numbers of one tenant's orders.</summary>
    /// <returns><see langword="false"/> when the call is worth retrying.</returns>
    private async Task<bool> IssueTenantAsync(string tenantId, string[] orderIds)
    {
        AutomationUser.OrderNumberAssigner.SignIn(tenantId);
        Error error;
        try
        {
            var result = await grainFactory.GetOrderNumberGeneratorGrain(tenantId).IssueOrderNumbersAsync(orderIds);
            if (result.IsSuccess)
            {
                return true;
            }
            error = result.Error;
        }
        catch (Exception exception)
        {
            LogIssueThrewRetrying(exception, tenantId, orderIds.Length);
            return false;
        }
        if (AutomationErrors.IsRetryable(error))
        {
            LogIssueFailedRetrying(tenantId, orderIds.Length, error);
            return false;
        }
        if (orderIds.Length == 1)
        {
            LogIssueFailedSkipped(orderIds[0], error);
            return true;
        }

        // One order the generator can never number (such as a corrupt ID) fails the whole call, so the orders are
        // issued one at a time to skip only that one.
        LogIssueFailedIssuingSeparately(tenantId, orderIds.Length, error);
        var allIssued = true;
        foreach (var orderId in orderIds)
        {
            allIssued &= await IssueTenantAsync(tenantId, [orderId]);
        }
        return allIssued;
    }

    private static string TenantOf(string orderId)
    {
        TenantKey.Split(orderId, out var tenantId, out _);
        return tenantId!;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Issuing the numbers of {OrderCount} orders of tenant {TenantId} failed and will be retried: {Error}")]
    private partial void LogIssueFailedRetrying(string tenantId, int orderCount, Error error);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Issuing the numbers of {OrderCount} orders of tenant {TenantId} failed and will be retried.")]
    private partial void LogIssueThrewRetrying(Exception exception, string tenantId, int orderCount);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Issuing the numbers of {OrderCount} orders of tenant {TenantId} failed, so they are issued one at a time: {Error}")]
    private partial void LogIssueFailedIssuingSeparately(string tenantId, int orderCount, Error error);

    [LoggerMessage(Level = LogLevel.Error, Message = "Order {OrderId} can never be issued a number and is skipped: {Error}")]
    private partial void LogIssueFailedSkipped(string orderId, Error error);
}
