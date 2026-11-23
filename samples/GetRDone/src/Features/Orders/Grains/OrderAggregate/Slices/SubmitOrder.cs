using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.OrderAggregate;

public sealed partial class OrderGrain
{
    public async Task<Result<SubmitOrderResponse>> SubmitOrderAsync()
    {
        return await Result.Success().BindTry(async () =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return await Result.CombineForValidation(
                    OrderId.Create(this.GetPrimaryKeyString(), tenantId).Deconstruct(out var orderId),
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId, nameof(OrderSubmitted.InitiatedBy)).Deconstruct(out var initiatedBy))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order draft was not found."))
                .Bind(() => _permissions.CanChangeOrder(initiatedBy!, State))
                .Ensure(() => State.StateMachine.CanFire(OrderCommand.SubmitOrder),
                    () => RequestErrors.NewFailedPrecondition("The order has already been submitted."))
                .Ensure(() => State.HasBorrower,
                    () => RequestErrors.NewFailedPrecondition("Save the borrower before submitting the order."))
                .BindWithStreamTransaction(this, transaction => transaction.Apply(new OrderSubmitted
                {
                    OrderId = orderId!,
                    ClientId = State.ClientId!,
                    InitiatedBy = initiatedBy!,
                    OccurredAt = DateTimeOffset.UtcNow
                }))
                .Map(response => new SubmitOrderResponse(response.Changes, response.Version));
        }, _grainCallErrorHandler);
    }
}

public sealed partial class OrderState
{
    private void Handle(OrderSubmitted evt) => Status = OrderStatus.Submitted;
}
