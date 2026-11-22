using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.OrderAggregate;

public sealed partial class OrderGrain
{
    public async Task<Result<SaveBorrowerAndAccessContactResponse>> SaveBorrowerAndAccessContactAsync(SaveBorrowerAndAccessContactCommand command)
    {
        return await Result.Success().BindTry(async () =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return await Result.CombineForValidation(
                    OrderId.Create(this.GetPrimaryKeyString(), tenantId).Deconstruct(out var orderId),
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId, nameof(BorrowerAndAccessContactSaved.InitiatedBy)).Deconstruct(out var initiatedBy),
                    BorrowerName.Create(command.BorrowerName).Deconstruct(out var borrowerName),
                    AccessContact.CreateOptional(command.AccessContactName, command.AccessContactPhone).Deconstruct(out var accessContact))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order draft was not found."))
                .Bind(() => _permissions.CanChangeOrder(initiatedBy!, State))
                .Ensure(() => State.StateMachine.CanFire(OrderCommand.SaveBorrowerAndAccessContact),
                    () => RequestErrors.NewFailedPrecondition("The order has been submitted and can no longer be changed."))
                .BindWithStreamTransaction(this, transaction => transaction.Apply(new BorrowerAndAccessContactSaved
                {
                    OrderId = orderId!,
                    BorrowerName = borrowerName!,
                    AccessContactName = accessContact?.Name.ToString(),
                    AccessContactPhone = accessContact?.Phone.ToString(),
                    InitiatedBy = initiatedBy!,
                    OccurredAt = DateTimeOffset.UtcNow
                }))
                .Map(response => new SaveBorrowerAndAccessContactResponse(response.Changes, response.Version));
        }, _grainCallErrorHandler);
    }
}
