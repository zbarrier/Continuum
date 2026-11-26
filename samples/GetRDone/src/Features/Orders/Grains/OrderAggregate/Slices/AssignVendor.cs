using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;

using ManagedCode.Orleans.Identity.Core.Extensions;

namespace GetRDone.Orders.Grains.OrderAggregate;

public sealed partial class OrderGrain
{
    public async Task<Result<AssignVendorResponse>> AssignVendorAsync(AssignVendorCommand command)
    {
        return await Result.Success().BindTry(async () =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return await Result.CombineForValidation(
                    OrderId.Create(this.GetPrimaryKeyString(), tenantId).Deconstruct(out var orderId),
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId, nameof(VendorAssigned.InitiatedBy)).Deconstruct(out var initiatedBy),
                    VendorId.Create(command.VendorId, tenantId, nameof(command.VendorId)).Deconstruct(out var vendorId),
                    VendorFee.Create(command.VendorFee, nameof(command.VendorFee)).Deconstruct(out var vendorFee),
                    VendorDueDate.Create(command.VendorDueDate, nameof(command.VendorDueDate)).Deconstruct(out var vendorDueDate))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order was not found."))
                .Bind(() => _permissions.CanAssignVendor(this.GetCurrentUser(), initiatedBy!, State))
                .Ensure(() => State.StateMachine.CanFire(OrderCommand.AssignVendor),
                    () => State.VendorAssignment is not null
                        ? RequestErrors.NewFailedPrecondition("The order is already assigned to a vendor.")
                        : RequestErrors.NewFailedPrecondition("The client must approve the description of work before the order is assigned to a vendor."))
                .Ensure(() => (DateOnly)vendorDueDate! >= DateOnly.FromDateTime(DateTime.UtcNow),
                    () => RequestErrors.NewInvalidArg("The vendor's due date can't be in the past."))
                .Bind(() => _permissions.CanBeAssignedVendorAsync(vendorId!))
                .BindWithStreamTransaction(this, transaction => transaction.Apply(new VendorAssigned
                {
                    OrderId = orderId!,
                    VendorId = vendorId!,
                    VendorFee = vendorFee!,
                    VendorDueDate = vendorDueDate!,
                    DescriptionOfWorkId = State.DescriptionOfWork!.Id,
                    DescriptionOfWorkVersion = State.DescriptionOfWork.Version,
                    OrderNumber = State.OrderNumber,
                    InitiatedBy = initiatedBy!,
                    OccurredAt = DateTimeOffset.UtcNow
                }))
                .Map(response => new AssignVendorResponse(response.Changes, response.Version));
        }, _grainCallErrorHandler);
    }
}

public sealed partial class OrderState
{
    private void Handle(VendorAssigned evt)
    {
        Status = OrderStatus.VendorAssigned;
        VendorAssignment = new OrderVendorAssignment(evt.VendorId, NoticeSent: false);
    }
}
