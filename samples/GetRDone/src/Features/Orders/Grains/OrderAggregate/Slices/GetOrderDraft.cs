using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.OrderAggregate;

public sealed partial class OrderGrain
{
    public async Task<Result<OrderDraft>> GetOrderDraftAsync()
    {
        return await Result.Success().BindTry(async () =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return await Result.CombineForValidation(
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId).Deconstruct(out var userId)
                )
                .Ensure(() => State.Status != OrderStatus.None, () => RequestErrors.NewNotFound("The order draft was not found."))
                .Bind(() => _permissions.CanViewOrder(userId!, State))
                .Ensure(() => State.Status == OrderStatus.Draft,
                    () => RequestErrors.NewFailedPrecondition("The order has been submitted, so it is no longer a draft."))
                .Map(async () => OrderDraft.Build(await RetrieveConfirmedEvents(0, Version)));
        }, _grainCallErrorHandler);
    }
}
