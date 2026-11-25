using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;

using ManagedCode.Orleans.Identity.Core.Extensions;

namespace GetRDone.Orders.Grains.OrderAggregate;

public sealed partial class OrderGrain
{
    public async Task<Result<AssignOrderResponse>> AssignOrderAsync(AssignOrderCommand command)
    {
        return await Result.Success().BindTry(async () =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return await Result.CombineForValidation(
                    OrderId.Create(this.GetPrimaryKeyString(), tenantId).Deconstruct(out var orderId),
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId, nameof(OrderAssigned.InitiatedBy)).Deconstruct(out var initiatedBy),
                    UserId.Create(command.AssignedToUserId, tenantId, nameof(AssignOrderCommand.AssignedToUserId)).Deconstruct(out var assignedTo))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order was not found."))
                .Bind(() => _permissions.CanAssignOrder(this.GetCurrentUser()))
                .Ensure(() => State.StateMachine.CanFire(OrderCommand.AssignOrder),
                    () => State.Status == OrderStatus.Draft
                        ? RequestErrors.NewFailedPrecondition("The order has not been submitted.")
                        : RequestErrors.NewFailedPrecondition("The order is already assigned."))
                .Bind(() => _permissions.CanBeAssignedOrderAsync(assignedTo!))
                .BindWithStreamTransaction(this, transaction => transaction.Apply(new OrderAssigned
                {
                    OrderId = orderId!,
                    AssignedToUserId = assignedTo!,
                    InitiatedBy = initiatedBy!,
                    OccurredAt = DateTimeOffset.UtcNow
                }))
                .Map(response => new AssignOrderResponse(response.Changes, response.Version));
        }, _grainCallErrorHandler);
    }
}

public sealed partial class OrderState
{
    private void Handle(OrderAssigned evt)
    {
        AssignedTo = evt.AssignedToUserId;
        Status = OrderStatus.Assigned;
    }
}
