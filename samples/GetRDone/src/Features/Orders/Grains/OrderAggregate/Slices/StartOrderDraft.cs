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
            TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);

            return await Result.CombineForValidation(
                    OrderId.Create(this.GetPrimaryKeyString(), tenantId).Deconstruct(out var orderId),
                    UserId.Create(GetCurrentUserId(tenantId), tenantId, nameof(OrderDraftStarted.InitiatedBy)).Deconstruct(out var initiatedBy),
                    ClientId.Create(command.ClientId, tenantId).Deconstruct(out var clientId),
                    ServiceRequestTypeId.Create(command.ServiceRequestTypeId, tenantId).Deconstruct(out var serviceRequestTypeId),
                    ServiceRequestTypeVersion.Create(command.ServiceRequestTypeVersion).Deconstruct(out var serviceRequestTypeVersion),
                    ServiceRequestTypeName.Create(command.ServiceRequestTypeName).Deconstruct(out var serviceRequestTypeName),
                    FormTypeId.Create(command.FormTypeId, tenantId).Deconstruct(out var formTypeId),
                    FormTypeVersion.Create(command.FormTypeVersion).Deconstruct(out var formTypeVersion),
                    FormTypeName.Create(command.FormTypeName).Deconstruct(out var formTypeName),
                    PropertyStreet.Create(command.PropertyStreet).Deconstruct(out var propertyStreet),
                    PropertyCity.Create(command.PropertyCity).Deconstruct(out var propertyCity),
                    PropertyState.Create(command.PropertyState).Deconstruct(out var propertyState),
                    PropertyZip.Create(command.PropertyZip).Deconstruct(out var propertyZip),
                    PropertyType.Create(command.PropertyType).Deconstruct(out var propertyType),
                    IntendedUseId.Create(command.IntendedUseId, tenantId).Deconstruct(out var intendedUseId),
                    IntendedUseVersion.Create(command.IntendedUseVersion).Deconstruct(out var intendedUseVersion),
                    IntendedUseName.Create(command.IntendedUseName).Deconstruct(out var intendedUseName),
                    LoanNumber.Create(command.LoanNumber).Deconstruct(out var loanNumber),
                    DueDate.Create(command.DueDate).Deconstruct(out var dueDate))
                .Ensure(() => State.StateMachine.CanFire(OrderCommand.StartDraft), 
                    () => RequestErrors.NewAlreadyExists("The order draft has already been started."))
                .Bind(() => _permissions.CanStartOrderAsync(initiatedBy!, clientId!))
                .Bind(() => GrainFactory.GetOrderableServicesGrain(tenantId).IsCurrentOrderableServiceAsync(
                    serviceRequestTypeId!, serviceRequestTypeVersion!, serviceRequestTypeName!,
                    formTypeId!, formTypeVersion!, formTypeName!))
                .Bind(() => GrainFactory.GetIntendedUsesGrain(tenantId).IsCurrentIntendedUseAsync(
                    intendedUseId!, intendedUseVersion!, intendedUseName!))
                .BindWithStreamTransaction(this, transaction => transaction.Apply(new OrderDraftStarted
                {
                    OrderId = orderId!,
                    ClientId = clientId!,
                    ServiceRequestTypeId = serviceRequestTypeId!,
                    ServiceRequestTypeVersion = serviceRequestTypeVersion!,
                    ServiceRequestTypeName = serviceRequestTypeName!,
                    FormTypeId = formTypeId!,
                    FormTypeVersion = formTypeVersion!,
                    FormTypeName = formTypeName!,
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
                    InitiatedBy = initiatedBy!,
                    OccurredAt = DateTimeOffset.UtcNow
                }))
                .Map(response => new StartOrderDraftResponse(response.Changes, response.Version));
        }, _grainCallErrorHandler);
    }
}


public sealed partial class OrderState
{
    private void Handle(OrderDraftStarted evt)
    {
        Status = OrderStatus.Draft;
        StartedBy = evt.InitiatedBy;
    }
}