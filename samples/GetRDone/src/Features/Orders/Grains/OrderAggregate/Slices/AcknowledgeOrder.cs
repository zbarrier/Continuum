using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.OrderAggregate;

public sealed partial class OrderGrain
{
    public async Task<Result<AcknowledgeOrderResponse>> AcknowledgeOrderAsync()
    {
        return await Result.Success().BindTry(async () =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return await Result.CombineForValidation(
                    OrderId.Create(this.GetPrimaryKeyString(), tenantId).Deconstruct(out var orderId),
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId, nameof(OrderAcknowledged.InitiatedBy)).Deconstruct(out var initiatedBy))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order was not found."))
                .Bind(() => _permissions.CanAcknowledgeOrder(initiatedBy!, State))
                .Ensure(() => State.StateMachine.CanFire(OrderCommand.AcknowledgeOrder),
                    () => State.Status == OrderStatus.Acknowledged
                        ? RequestErrors.NewFailedPrecondition("The order is already acknowledged.")
                        : RequestErrors.NewFailedPrecondition("The order is not assigned yet."))
                .BindWithStreamTransaction(this, transaction => transaction.Apply(new OrderAcknowledged
                {
                    OrderId = orderId!,
                    InitiatedBy = initiatedBy!,
                    OccurredAt = DateTimeOffset.UtcNow
                }))
                .Map(response => new AcknowledgeOrderResponse(response.Changes, response.Version));
        }, _grainCallErrorHandler);
    }
}

public sealed partial class OrderState
{
    private void Handle(OrderAcknowledged evt) => Status = OrderStatus.Acknowledged;
}
