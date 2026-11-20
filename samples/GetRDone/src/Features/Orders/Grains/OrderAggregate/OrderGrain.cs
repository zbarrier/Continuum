using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;

using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Abstractions.ReadModels;

using Microsoft.AspNetCore.Authorization;

using Orleans.Providers;

namespace GetRDone.Orders.Grains.OrderAggregate;

[Authorize]
[GrainType("order")]
[LogConsistencyProvider(ProviderName = "EventLog")]
public sealed partial class OrderGrain(IServiceProvider serviceProvider)
    : EventDrivenGrain<OrderState, OrderEvent>(serviceProvider, "kurrentdb"), IOrderGrain
{ }

[GenerateSerializer]
public sealed class OrderState : EventDrivenState<OrderState, OrderEvent>
{
    public OrderState() => On<OrderDraftStarted>(Apply);

    [Id(0)]
    public OrderDraftStarted? Draft { get; set; }

    private void Apply(OrderDraftStarted @event) => Draft = @event;
}

public static class StartOrderDraft
{
    public static Result<OrderDraftStarted> Decide(
        OrderState state, StartOrderDraftCommand command, IReadOnlyList<OrderableService> offerings,
        Guid orderId, Guid initiatedBy, DateTimeOffset occurredAt)
    {
        if (state.Draft is not null)
            return Result.Failure<OrderDraftStarted>(RequestErrors.NewAlreadyExists("The order already exists."));

        if (string.IsNullOrWhiteSpace(command.PropertyStreet) || string.IsNullOrWhiteSpace(command.PropertyCity)
            || string.IsNullOrWhiteSpace(command.PropertyState) || string.IsNullOrWhiteSpace(command.PropertyZip))
            return Result.Failure<OrderDraftStarted>(RequestErrors.NewInvalidArg("A complete property address is required."));

        if (string.IsNullOrWhiteSpace(command.PropertyType) || string.IsNullOrWhiteSpace(command.IntendedUseId)
            || string.IsNullOrWhiteSpace(command.IntendedUseVersion) || string.IsNullOrWhiteSpace(command.IntendedUseName)
            || string.IsNullOrWhiteSpace(command.LoanNumber) || command.DueDate == default)
            return Result.Failure<OrderDraftStarted>(RequestErrors.NewInvalidArg("The request and property details are required."));

        var offering = offerings.FirstOrDefault(item => item.ServiceRequestTypeId == command.ServiceRequestTypeId
            && item.ServiceRequestTypeVersion == command.ServiceRequestTypeVersion
            && item.FormTypeId == command.FormTypeId && item.FormTypeVersion == command.FormTypeVersion);
        if (offering is null)
            return Result.Failure<OrderDraftStarted>(RequestErrors.NewInvalidArg("The selected service and form are not offered."));

        return Result.Success(new OrderDraftStarted
        {
            OrderId = orderId,
            ServiceRequestTypeId = offering.ServiceRequestTypeId,
            ServiceRequestTypeVersion = offering.ServiceRequestTypeVersion,
            ServiceRequestTypeName = offering.ServiceRequestTypeName,
            FormTypeId = offering.FormTypeId,
            FormTypeVersion = offering.FormTypeVersion,
            FormTypeName = offering.FormTypeName,
            PropertyStreet = command.PropertyStreet,
            PropertyCity = command.PropertyCity,
            PropertyState = command.PropertyState,
            PropertyZip = command.PropertyZip,
            PropertyType = command.PropertyType,
            IntendedUseId = command.IntendedUseId,
            IntendedUseVersion = command.IntendedUseVersion,
            IntendedUseName = command.IntendedUseName,
            LoanNumber = command.LoanNumber,
            DueDate = command.DueDate,
            InitiatedBy = initiatedBy,
            OccurredAt = occurredAt
        });
    }
}