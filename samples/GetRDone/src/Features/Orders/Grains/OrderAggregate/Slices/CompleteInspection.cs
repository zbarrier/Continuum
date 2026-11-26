using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.OrderAggregate;

public sealed partial class OrderGrain
{
    public async Task<Result<CompleteInspectionResponse>> CompleteInspectionAsync(CompleteInspectionCommand command)
    {
        return await Result.Success().BindTry(async () =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return await Result.CombineForValidation(
                    OrderId.Create(this.GetPrimaryKeyString(), tenantId).Deconstruct(out var orderId),
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId, nameof(InspectionCompleted.InitiatedBy)).Deconstruct(out var initiatedBy),
                    InspectionCompletedOn.Create(command.CompletedOn, nameof(command.CompletedOn)).Deconstruct(out var completedOn))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order was not found."))
                .Ensure(() => State.VendorAssignment is not null,
                    () => RequestErrors.NewFailedPrecondition("The order is not assigned to a vendor yet."))
                .Bind(() => _permissions.CanDoVendorWorkAsync(initiatedBy!, State))
                .Ensure(() => State.StateMachine.CanFire(OrderCommand.CompleteInspection),
                    () => State.InspectionCompletedOn is not null
                        ? RequestErrors.NewFailedPrecondition("The inspection is already completed.")
                        : RequestErrors.NewFailedPrecondition("The inspection must be scheduled before it is completed."))
                .Ensure(() => (DateOnly)completedOn! <= DateOnly.FromDateTime(DateTime.UtcNow),
                    () => RequestErrors.NewInvalidArg("The inspection can't be completed in the future."))
                .BindWithStreamTransaction(this, transaction => transaction.Apply(new InspectionCompleted
                {
                    OrderId = orderId!,
                    CompletedOn = completedOn!,
                    InitiatedBy = initiatedBy!,
                    OccurredAt = DateTimeOffset.UtcNow
                }))
                .Map(response => new CompleteInspectionResponse(response.Changes, response.Version));
        }, _grainCallErrorHandler);
    }
}

public sealed partial class OrderState
{
    private void Handle(InspectionCompleted evt)
    {
        Status = OrderStatus.InspectionCompleted;
        InspectionCompletedOn = evt.CompletedOn;
    }
}
