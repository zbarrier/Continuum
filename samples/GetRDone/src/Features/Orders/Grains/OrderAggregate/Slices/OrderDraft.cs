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
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId).Deconstruct(out var userId))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order draft was not found."))
                .Bind(() => _permissions.CanViewOrder(userId!, State))
                .Map(async () => OrderDraftReadModel.Build(await RetrieveConfirmedEvents(0, Version)));
        }, _grainCallErrorHandler);
    }
}

/// <summary>Builds the OrderDraft read model by applying an order's events in order.</summary>
internal static class OrderDraftReadModel
{
    /// <exception cref="InvalidOperationException">The events do not start with <see cref="OrderDraftStarted"/>.</exception>
    public static OrderDraft Build(IEnumerable<OrderEvent> events)
    {
        OrderDraft? draft = null;
        foreach (var evt in events)
        {
            draft = evt switch
            {
                OrderDraftStarted started => new OrderDraft(
                    started.OrderId,
                    started.ServiceRequestTypeId,
                    started.ServiceRequestTypeVersion,
                    started.ServiceRequestTypeName,
                    started.FormTypeId,
                    started.FormTypeVersion,
                    started.FormTypeName,
                    started.PropertyStreet,
                    started.PropertyCity,
                    started.PropertyState,
                    started.PropertyZip,
                    started.PropertyType,
                    started.IntendedUseId,
                    started.IntendedUseVersion,
                    started.IntendedUseName,
                    started.LoanNumber,
                    started.DueDate,
                    BorrowerName: null,
                    AccessContactName: null,
                    AccessContactPhone: null,
                    started.OccurredAt),
                RequestAndPropertySaved saved => Started(draft) with
                {
                    ServiceRequestTypeId = saved.ServiceRequestTypeId,
                    ServiceRequestTypeVersion = saved.ServiceRequestTypeVersion,
                    ServiceRequestTypeName = saved.ServiceRequestTypeName,
                    FormTypeId = saved.FormTypeId,
                    FormTypeVersion = saved.FormTypeVersion,
                    FormTypeName = saved.FormTypeName,
                    PropertyStreet = saved.PropertyStreet,
                    PropertyCity = saved.PropertyCity,
                    PropertyState = saved.PropertyState,
                    PropertyZip = saved.PropertyZip,
                    PropertyType = saved.PropertyType,
                    IntendedUseId = saved.IntendedUseId,
                    IntendedUseVersion = saved.IntendedUseVersion,
                    IntendedUseName = saved.IntendedUseName,
                    LoanNumber = saved.LoanNumber,
                    DueDate = saved.DueDate,
                    LastSavedAt = saved.OccurredAt
                },
                BorrowerAndAccessContactSaved saved => Started(draft) with
                {
                    BorrowerName = saved.BorrowerName,
                    AccessContactName = saved.AccessContactName,
                    AccessContactPhone = saved.AccessContactPhone,
                    LastSavedAt = saved.OccurredAt
                },
                _ => draft
            };
        }
        return Started(draft);
    }

    private static OrderDraft Started(OrderDraft? draft)
        => draft ?? throw new InvalidOperationException($"An order's events must start with {nameof(OrderDraftStarted)}.");
}
