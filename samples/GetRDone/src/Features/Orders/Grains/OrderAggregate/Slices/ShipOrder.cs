using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;

using ManagedCode.Orleans.Identity.Core.Extensions;

namespace GetRDone.Orders.Grains.OrderAggregate;

public sealed partial class OrderGrain
{
    public async Task<Result<ShipOrderResponse>> ShipOrderAsync(ShipOrderCommand command)
    {
        return await Result.Success().BindTry(async () =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return await Result.CombineForValidation(
                    OrderId.Create(this.GetPrimaryKeyString(), tenantId).Deconstruct(out var orderId),
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId, nameof(OrderShipped.InitiatedBy)).Deconstruct(out var initiatedBy),
                    DocumentId.Create(command.DocumentId, tenantId, nameof(command.DocumentId)).Deconstruct(out var documentId))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order was not found."))
                .Bind(() => _permissions.CanShipOrder(this.GetCurrentUser(), initiatedBy!, State))
                .Ensure(() => State.StateMachine.CanFire(OrderCommand.ShipOrder),
                    () => State.Status == OrderStatus.Shipped
                        ? RequestErrors.NewFailedPrecondition("The order was already shipped.")
                        : RequestErrors.NewFailedPrecondition("The report must be delivered before the order is shipped."))
                .Ensure(() => State.Report?.DocumentId == (string)documentId!,
                    () => RequestErrors.NewNotFound("The report was not found."))
                .BindWithStreamTransaction(this, transaction => transaction.Apply(new OrderShipped
                {
                    OrderId = orderId!,
                    DocumentId = documentId!,
                    EnteredByUserId = State.StartedBy!,
                    OrderNumber = State.OrderNumber,
                    InitiatedBy = initiatedBy!,
                    OccurredAt = DateTimeOffset.UtcNow
                }))
                .Map(response => new ShipOrderResponse(response.Changes, response.Version));
        }, _grainCallErrorHandler);
    }
}

public sealed partial class OrderState
{
    private void Handle(OrderShipped evt) => Status = OrderStatus.Shipped;
}
