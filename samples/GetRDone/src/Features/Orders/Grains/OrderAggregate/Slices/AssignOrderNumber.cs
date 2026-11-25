using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;

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
}

public sealed partial class OrderState
{
    private void Handle(OrderNumberAssigned evt) => OrderNumber = evt.OrderNumber;
}
