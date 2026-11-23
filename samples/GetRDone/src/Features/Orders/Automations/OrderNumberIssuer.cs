using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Abstractions.OrderNumbers;

using Microsoft.Extensions.Logging;

namespace GetRDone.Orders.Automations;

/// <summary>
///     The first step of giving submitted orders their numbers: asks each tenant's order number generator to issue
///     numbers to the orders submitted in a batch. The <see cref="OrderNumberAssigner"/> then gives each order the
///     number it was issued.
/// </summary>
/// <remarks>
///     Runs as <see cref="AutomationUser.OrderNumberAssigner"/>. The generator issues an order only one number, so
///     retrying a batch that was partly issued issues nothing twice.
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
        try
        {
            var result = await grainFactory.GetOrderNumberGeneratorGrain(tenantId).IssueOrderNumbersAsync(orderIds);
            if (result.IsSuccess)
            {
                return true;
            }
            if (OrderNumbersPendingException.IsRetryable(result.Error))
            {
                LogIssueFailedRetrying(tenantId, orderIds.Length, result.Error);
                return false;
            }
            LogIssueFailedSkipped(tenantId, orderIds.Length, result.Error);
            return true;
        }
        catch (Exception exception)
        {
            LogIssueThrewRetrying(exception, tenantId, orderIds.Length);
            return false;
        }
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

    [LoggerMessage(Level = LogLevel.Error, Message = "The {OrderCount} orders of tenant {TenantId} were not issued numbers and are skipped: {Error}")]
    private partial void LogIssueFailedSkipped(string tenantId, int orderCount, Error error);
}
