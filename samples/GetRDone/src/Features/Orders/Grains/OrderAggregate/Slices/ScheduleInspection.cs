using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.OrderAggregate;

public sealed partial class OrderGrain
{
    public async Task<Result<ScheduleInspectionResponse>> ScheduleInspectionAsync(ScheduleInspectionCommand command)
    {
        return await Result.Success().BindTry(async () =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return await Result.CombineForValidation(
                    OrderId.Create(this.GetPrimaryKeyString(), tenantId).Deconstruct(out var orderId),
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId, nameof(InspectionScheduled.InitiatedBy)).Deconstruct(out var initiatedBy),
                    InspectionAt.Create(command.InspectionAt, nameof(command.InspectionAt)).Deconstruct(out var inspectionAt),
                    InspectionContactName.Create(command.ContactName, nameof(command.ContactName)).Deconstruct(out var contactName))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order was not found."))
                .Ensure(() => State.VendorAssignment is not null,
                    () => RequestErrors.NewFailedPrecondition("The order is not assigned to a vendor yet."))
                .Bind(() => _permissions.CanDoVendorWorkAsync(initiatedBy!, State))
                .Ensure(() => State.StateMachine.CanFire(OrderCommand.ScheduleInspection),
                    () => RequestErrors.NewFailedPrecondition("The inspection is already scheduled."))
                .Ensure(() => (DateTimeOffset)inspectionAt! >= DateTimeOffset.UtcNow,
                    () => RequestErrors.NewInvalidArg("The inspection can't be scheduled in the past."))
                .BindWithStreamTransaction(this, transaction => transaction.Apply(new InspectionScheduled
                {
                    OrderId = orderId!,
                    InspectionAt = inspectionAt!,
                    ContactName = contactName!,
                    InitiatedBy = initiatedBy!,
                    OccurredAt = DateTimeOffset.UtcNow
                }))
                .Map(response => new ScheduleInspectionResponse(response.Changes, response.Version));
        }, _grainCallErrorHandler);
    }
}

public sealed partial class OrderState
{
    private void Handle(InspectionScheduled evt)
    {
        Status = OrderStatus.InspectionScheduled;
        InspectionAt = evt.InspectionAt;
    }
}
