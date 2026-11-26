using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.OrderAggregate;

public sealed partial class OrderGrain
{
    public async Task<Result<ApproveDescriptionOfWorkResponse>> ApproveDescriptionOfWorkAsync(ApproveDescriptionOfWorkCommand command)
    {
        return await Result.Success().BindTry(async () =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return await Result.CombineForValidation(
                    OrderId.Create(this.GetPrimaryKeyString(), tenantId).Deconstruct(out var orderId),
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId, nameof(DescriptionOfWorkApproved.InitiatedBy)).Deconstruct(out var initiatedBy),
                    DescriptionOfWorkId.Create(command.DescriptionOfWorkId, tenantId, nameof(command.DescriptionOfWorkId))
                        .Deconstruct(out var descriptionOfWorkId),
                    DescriptionOfWorkVersion.Create(command.DescriptionOfWorkVersion, nameof(command.DescriptionOfWorkVersion))
                        .Deconstruct(out var version))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order was not found."))
                .Ensure(() => State.DescriptionOfWork?.Id == (string)descriptionOfWorkId!,
                    () => RequestErrors.NewNotFound("The description of work was not found."))
                .Bind(() => _permissions.CanApproveDescriptionOfWork(initiatedBy!, State.DescriptionOfWork!))
                .Ensure(() => State.StateMachine.CanFire(OrderCommand.ApproveDescriptionOfWork),
                    () => State.Status == OrderStatus.DescriptionOfWorkApproved
                        ? RequestErrors.NewFailedPrecondition("The description of work was already approved.")
                        : RequestErrors.NewFailedPrecondition("No description of work waits for approval."))
                .Ensure(() => State.DescriptionOfWork!.Version == (int)version!,
                    () => RequestErrors.NewFailedPrecondition("The description of work was revised. Review the latest version and approve it."))
                .BindWithStreamTransaction(this, transaction => transaction.Apply(new DescriptionOfWorkApproved
                {
                    OrderId = orderId!,
                    DescriptionOfWorkId = descriptionOfWorkId!,
                    DescriptionOfWorkVersion = (int)version!,
                    AssignedToUserId = State.AssignedTo!,
                    OrderNumber = State.OrderNumber,
                    InitiatedBy = initiatedBy!,
                    OccurredAt = DateTimeOffset.UtcNow
                }))
                .Map(response => new ApproveDescriptionOfWorkResponse(response.Changes, response.Version));
        }, _grainCallErrorHandler);
    }
}

public sealed partial class OrderState
{
    private void Handle(DescriptionOfWorkApproved evt)
    {
        Status = OrderStatus.DescriptionOfWorkApproved;
        DescriptionOfWork = DescriptionOfWork! with { Approved = true };
    }
}
