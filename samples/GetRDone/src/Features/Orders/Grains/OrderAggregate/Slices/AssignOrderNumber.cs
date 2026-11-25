using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Abstractions.OrderNumbers;

using ManagedCode.Orleans.Identity.Core.Constants;

using Microsoft.Extensions.Logging;

namespace GetRDone.Orders.Grains.OrderAggregate;

public sealed partial class OrderGrain
{
    public async Task<Result<AssignOrderNumberResponse>> AssignOrderNumberAsync(long orderNumber)
    {
        return await Result.Success().BindTry(async () =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return await Result.CombineForValidation(
                    OrderId.Create(this.GetPrimaryKeyString(), tenantId).Deconstruct(out var orderId),
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId, nameof(OrderNumberAssigned.InitiatedBy)).Deconstruct(out var initiatedBy),
                    OrderNumber.Create(orderNumber).Deconstruct(out var number))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order was not found."))
                .Bind(() => _permissions.CanAssignOrderNumber(initiatedBy!))
                .Ensure(() => State.OrderNumber is not null || State.StateMachine.CanFire(OrderCommand.AssignOrderNumber),
                    () => RequestErrors.NewFailedPrecondition("The order has not been submitted."))
                .BindWithStreamTransaction(this, transaction =>
                {
                    // The order keeps the first number it was assigned. The generator issues an order only one number
                    // within its recent events, so another attempt is an earlier attempt of the automation or, rarely,
                    // a number issued again after the generator no longer kept the order's first one.
                    if (State.OrderNumber is not null)
                    {
                        return;
                    }
                    transaction.Apply(new OrderNumberAssigned
                    {
                        OrderId = orderId!,
                        OrderNumber = number!,
                        ClientId = State.ClientId!,
                        InitiatedBy = initiatedBy!,
                        OccurredAt = DateTimeOffset.UtcNow
                    });
                })
                .Map(response => new AssignOrderNumberResponse(response.Changes, response.Version));
        }, _grainCallErrorHandler);
    }

    /// <summary>
    ///     Gives the order the number its tenant's generator issued it, signed in as
    ///     <see cref="AutomationUser.OrderNumberAssigner"/>.
    /// </summary>
    /// <remarks>
    ///     An order that already has its number succeeds without changes, so an event delivered again is harmless. A
    ///     failure that may pass (see <see cref="AutomationErrors.IsRetryable"/>) throws, so the event is delivered again
    ///     until the order has its number; one that never will, such as an order that was not submitted, is logged and
    ///     skipped.
    /// </remarks>
    /// <exception cref="OrderNumberNotAssignedException">The order was not given its number for a reason that may pass.</exception>
    private async Task ReactToOrderNumberIssuedAsync(OrderNumberIssued issued)
    {
        var orderId = this.GetPrimaryKeyString();
        TenantKey.Split(orderId, out var tenantId, out _);
        if (tenantId is null || !string.Equals(issued.OrderId, orderId, StringComparison.Ordinal))
        {
            LogOrderNumberIssuedToAnotherOrder(orderId, issued.OrderId, issued.OrderNumber);
            return;
        }

        Result<AssignOrderNumberResponse> result;
        RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, AutomationUser.OrderNumberAssigner.CreatePrincipal(tenantId));
        try
        {
            result = await AssignOrderNumberAsync(issued.OrderNumber);
        }
        finally
        {
            RequestContext.Remove(OrleansIdentityConstants.USER_CLAIMS);
        }

        if (result.IsSuccess)
        {
            return;
        }
        if (AutomationErrors.IsRetryable(result.Error))
        {
            LogOrderNumberNotAssignedRetrying(orderId, issued.OrderNumber, result.Error);
            throw new OrderNumberNotAssignedException($"Order {orderId} was not given order number {issued.OrderNumber} and will be retried.");
        }
        LogOrderNumberNotAssignedSkipped(orderId, issued.OrderNumber, result.Error);
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Order {OrderId} ignored order number {OrderNumber}, which was issued to order {IssuedOrderId}.")]
    private partial void LogOrderNumberIssuedToAnotherOrder(string orderId, string issuedOrderId, long orderNumber);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Giving order {OrderId} order number {OrderNumber} failed and will be retried: {Error}")]
    private partial void LogOrderNumberNotAssignedRetrying(string orderId, long orderNumber, Error error);

    [LoggerMessage(Level = LogLevel.Error, Message = "Order {OrderId} can never be given order number {OrderNumber} and is skipped: {Error}")]
    private partial void LogOrderNumberNotAssignedSkipped(string orderId, long orderNumber, Error error);
}

public sealed partial class OrderState
{
    private void Handle(OrderNumberAssigned evt) => OrderNumber = evt.OrderNumber;
}
