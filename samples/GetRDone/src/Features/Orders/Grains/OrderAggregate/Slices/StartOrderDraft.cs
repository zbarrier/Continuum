using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Abstractions.ReadModels;

namespace GetRDone.Orders.Grains.OrderAggregate;

public sealed partial class OrderGrain
{
    public async Task<Result<StartOrderDraftResponse>> StartOrderDraftAsync(StartOrderDraftCommand command)
    {
        return await Result.Success().BindTry(async () =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out var key);

            return await Result.CombineForValidation(
                    OrderId.Create(key).Deconstruct(out var orderId),
                    UserId.Create(GetCurrentUserId(), "RequestedBy").Deconstruct(out var requestedBy),
                    ServiceRequestTypeId.Create(command.ServiceRequestTypeId).Deconstruct(out var serviceRequestTypeId),
                    ServiceRequestTypeVersion.Create(command.ServiceRequestTypeVersion).Deconstruct(out var serviceRequestTypeVersion),
                    FormTypeId.Create(command.FormTypeId).Deconstruct(out var formTypeId),
                    FormTypeVersion.Create(command.FormTypeVersion).Deconstruct(out var formTypeVersion),
                    PropertyStreet.Create(command.PropertyStreet).Deconstruct(out var propertyStreet),
                    PropertyCity.Create(command.PropertyCity).Deconstruct(out var propertyCity),
                    PropertyState.Create(command.PropertyState).Deconstruct(out var propertyState),
                    PropertyZip.Create(command.PropertyZip).Deconstruct(out var propertyZip),
                    PropertyType.Create(command.PropertyType).Deconstruct(out var propertyType),
                    IntendedUseId.Create(command.IntendedUseId).Deconstruct(out var intendedUseId),
                    IntendedUseVersion.Create(command.IntendedUseVersion).Deconstruct(out var intendedUseVersion),
                    IntendedUseName.Create(command.IntendedUseName).Deconstruct(out var intendedUseName),
                    LoanNumber.Create(command.LoanNumber).Deconstruct(out var loanNumber),
                    DueDate.Create(command.DueDate).Deconstruct(out var dueDate))
                .Ensure(() => State.StateMachine.CanFire(OrderCommand.StartDraft),
                    () => RequestErrors.NewAlreadyExists("The order draft has already been started."))
                .BindWithStreamTransaction(this, async transaction =>
                {
                    var offerings = await GrainFactory.GetGrain<IOrderableServicesGrain>(TenantKey.Create(tenantId, "offerings"))
                        .GetOfferingsAsync();
                    var offering = offerings.FirstOrDefault(item =>
                        item.ServiceRequestTypeId == (Guid)serviceRequestTypeId!
                        && item.ServiceRequestTypeVersion == (int)serviceRequestTypeVersion!
                        && item.FormTypeId == (Guid)formTypeId!
                        && item.FormTypeVersion == (int)formTypeVersion!);
                    if (offering is null)
                    {
                        return Result.Failure(RequestErrors.NewInvalidArg("The selected service and form are not offered."));
                    }

                    transaction.Apply(new OrderDraftStarted
                    {
                        OrderId = orderId!,
                        ServiceRequestTypeId = offering.ServiceRequestTypeId,
                        ServiceRequestTypeVersion = offering.ServiceRequestTypeVersion,
                        ServiceRequestTypeName = offering.ServiceRequestTypeName,
                        FormTypeId = offering.FormTypeId,
                        FormTypeVersion = offering.FormTypeVersion,
                        FormTypeName = offering.FormTypeName,
                        PropertyStreet = propertyStreet!,
                        PropertyCity = propertyCity!,
                        PropertyState = propertyState!,
                        PropertyZip = propertyZip!,
                        PropertyType = propertyType!,
                        IntendedUseId = intendedUseId!,
                        IntendedUseVersion = intendedUseVersion!,
                        IntendedUseName = intendedUseName!,
                        LoanNumber = loanNumber!,
                        DueDate = dueDate!,
                        InitiatedBy = requestedBy!,
                        OccurredAt = DateTimeOffset.UtcNow
                    });
                    return Result.Success();
                })
                .Map(response => new StartOrderDraftResponse(response.Changes, response.Version));
        }, _grainCallErrorHandler);
    }
}
