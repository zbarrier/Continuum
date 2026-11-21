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
                    UserId.Create(this.GetCurrentTenantUserId(tenantId), tenantId, nameof(OrderDraftStarted.InitiatedBy)).Deconstruct(out var initiatedBy),
                    ClientId.Create(command.ClientId, tenantId).Deconstruct(out var clientId),
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
                .Ensure(() => State.StateMachine.CanFire(OrderCommand.StartDraft), 
                    () => RequestErrors.NewAlreadyExists("The order draft has already been started."))
                .Bind(() => _permissions.CanStartOrderAsync(initiatedBy!, clientId!))
                .Bind(() => GrainFactory.GetOrderableServicesGrain(tenantId).IsCurrentOrderableServiceAsync(serviceRequestType!, formType!))
                .Bind(() => GrainFactory.GetIntendedUsesGrain(tenantId).IsCurrentIntendedUseAsync(intendedUse!))
                .BindWithStreamTransaction(this, transaction => transaction.Apply(new OrderDraftStarted
                {
                    OrderId = orderId!,
                    ClientId = clientId!,
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