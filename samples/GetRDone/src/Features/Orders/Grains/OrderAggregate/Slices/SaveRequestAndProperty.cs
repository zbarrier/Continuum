using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Abstractions.ReadModels;

namespace GetRDone.Orders.Grains.OrderAggregate;

public sealed partial class OrderGrain
{
    public async Task<Result<SaveRequestAndPropertyResponse>> SaveRequestAndPropertyAsync(SaveRequestAndPropertyCommand command)
    {
        return await Result.Success().BindTry(async () =>
        {
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return await Result.CombineForValidation(
                    OrderId.Create(this.GetPrimaryKeyString(), tenantId).Deconstruct(out var orderId),
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId, nameof(RequestAndPropertySaved.InitiatedBy)).Deconstruct(out var initiatedBy),
                    ServiceRequestTypeRef.Create(command.ServiceRequestTypeId, command.ServiceRequestTypeVersion, command.ServiceRequestTypeName, tenantId)
                        .Deconstruct(out var serviceRequestType),
                    FormTypeRef.Create(command.FormTypeId, command.FormTypeVersion, command.FormTypeName, tenantId)
                        .Deconstruct(out var formType),
                    PropertyAddress.Create(command.PropertyStreet, command.PropertyCity, command.PropertyState, command.PropertyZip)
                        .Deconstruct(out var propertyAddress),
                    PropertyType.Create(command.PropertyType).Deconstruct(out var propertyType),
                    IntendedUseRef.Create(command.IntendedUseId, command.IntendedUseVersion, command.IntendedUseName, tenantId)
                        .Deconstruct(out var intendedUse),
                    LoanNumber.Create(command.LoanNumber).Deconstruct(out var loanNumber),
                    DueDate.Create(command.DueDate).Deconstruct(out var dueDate))
                .Ensure(() => State.Status != OrderStatus.None,
                    () => RequestErrors.NewNotFound("The order draft was not found."))
                .Bind(() => _permissions.CanChangeOrder(initiatedBy!, State))
                .Ensure(() => State.StateMachine.CanFire(OrderCommand.SaveRequestAndProperty),
                    () => RequestErrors.NewFailedPrecondition("The order has been submitted and can no longer be changed."))
                .Bind(() => GrainFactory.GetOrderableServicesGrain(tenantId).IsCurrentOrderableServiceAsync(serviceRequestType!, formType!))
                .Bind(() => GrainFactory.GetIntendedUsesGrain(tenantId).IsCurrentIntendedUseAsync(intendedUse!))
                .BindWithStreamTransaction(this, transaction => transaction.Apply(new RequestAndPropertySaved
                {
                    OrderId = orderId!,
                    ServiceRequestTypeId = serviceRequestType!.Id,
                    ServiceRequestTypeVersion = serviceRequestType.Version,
                    ServiceRequestTypeName = serviceRequestType.Name,
                    FormTypeId = formType!.Id,
                    FormTypeVersion = formType.Version,
                    FormTypeName = formType.Name,
                    PropertyStreet = propertyAddress!.Street,
                    PropertyCity = propertyAddress.City,
                    PropertyState = propertyAddress.State,
                    PropertyZip = propertyAddress.Zip,
                    PropertyType = propertyType!,
                    IntendedUseId = intendedUse!.Id,
                    IntendedUseVersion = intendedUse.Version,
                    IntendedUseName = intendedUse.Name,
                    LoanNumber = loanNumber!,
                    DueDate = dueDate!,
                    InitiatedBy = initiatedBy!,
                    OccurredAt = DateTimeOffset.UtcNow
                }))
                .Map(response => new SaveRequestAndPropertyResponse(response.Changes, response.Version));
        }, _grainCallErrorHandler);
    }
}
