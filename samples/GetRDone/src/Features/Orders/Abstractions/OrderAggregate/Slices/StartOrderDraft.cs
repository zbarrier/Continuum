using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

namespace GetRDone.Orders.Abstractions.OrderAggregate;

public partial interface IOrderGrain : IEventDrivenGrain
{
    Task<Result<StartOrderDraftResponse>> StartOrderDraftAsync(StartOrderDraftCommand command);
}

[GenerateSerializer, Immutable]
public sealed class StartOrderDraftCommand(
    Guid serviceRequestTypeId,
    int serviceRequestTypeVersion,
    Guid formTypeId,
    int formTypeVersion,
    string propertyStreet,
    string propertyCity,
    string propertyState,
    string propertyZip,
    string propertyType,
    string intendedUseId,
    string intendedUseVersion,
    string intendedUseName,
    string loanNumber,
    DateOnly dueDate)
{
    
    [Id(0)] public Guid ServiceRequestTypeId { get; init; } = serviceRequestTypeId;
    [Id(1)] public int ServiceRequestTypeVersion { get; init; } = serviceRequestTypeVersion;
    [Id(2)] public Guid FormTypeId { get; init; } = formTypeId;
    [Id(3)] public int FormTypeVersion { get; init; } = formTypeVersion;
    [Id(4)] public string PropertyStreet { get; init; } = propertyStreet;
    [Id(5)] public string PropertyCity { get; init; } = propertyCity;
    [Id(6)] public string PropertyState { get; init; } = propertyState;
    [Id(7)] public string PropertyZip { get; init; } = propertyZip;
    [Id(8)] public string PropertyType { get; init; } = propertyType;
    [Id(9)] public string IntendedUseId { get; init; } = intendedUseId;
    [Id(10)] public string IntendedUseVersion { get; init; } = intendedUseVersion;
    [Id(11)] public string IntendedUseName { get; init; } = intendedUseName;
    [Id(12)] public string LoanNumber { get; init; } = loanNumber;
    [Id(13)] public DateOnly DueDate { get; init; } = dueDate;
}

[GenerateSerializer, Immutable]
public sealed class StartOrderDraftResponse(IReadOnlyList<IChange> changes, int version) : CommandResponseWithChangesBase(changes, version);

[DomainEventType("GetRDone.Orders.OrderDraftStarted"), GenerateSerializer, Immutable]
public sealed class OrderDraftStarted : OrderEvent
{
    public OrderDraftStarted(Guid orderId, Guid serviceRequestTypeId, int serviceRequestTypeVersion, string serviceRequestTypeName, Guid formTypeId, int formTypeVersion, string formTypeName, 
        string propertyStreet, string propertyCity, string propertyState, string propertyZip, string propertyType, string intendedUseId, string intendedUseVersion, string intendedUseName, 
        string loanNumber, DateOnly dueDate, Guid initiatedBy, DateTimeOffset occurredAt)
        : base(initiatedBy, occurredAt)
    {
        OrderId = orderId;
        ServiceRequestTypeId = serviceRequestTypeId;
        ServiceRequestTypeVersion = serviceRequestTypeVersion;
        ServiceRequestTypeName = serviceRequestTypeName;
        FormTypeId = formTypeId;
        FormTypeVersion = formTypeVersion;
        FormTypeName = formTypeName;
        PropertyStreet = propertyStreet;
        PropertyCity = propertyCity;
        PropertyState = propertyState;
        PropertyZip = propertyZip;
        PropertyType = propertyType;
        IntendedUseId = intendedUseId;
        IntendedUseVersion = intendedUseVersion;
        IntendedUseName = intendedUseName;
        LoanNumber = loanNumber;
        DueDate = dueDate;
    }

    [Id(0)] public Guid OrderId { get; init; }
    [Id(1)] public Guid ServiceRequestTypeId { get; init; }
    [Id(2)] public int ServiceRequestTypeVersion { get; init; }
    [Id(3)] public string ServiceRequestTypeName { get; init; }
    [Id(4)] public Guid FormTypeId { get; init; }
    [Id(5)] public int FormTypeVersion { get; init; }
    [Id(6)] public string FormTypeName { get; init; }
    [Id(7)] public string PropertyStreet { get; init; }
    [Id(8)] public string PropertyCity { get; init; }
    [Id(9)] public string PropertyState { get; init; }
    [Id(10)] public string PropertyZip { get; init; }
    [Id(11)] public string PropertyType { get; init; }
    [Id(12)] public string IntendedUseId { get; init; }
    [Id(13)] public string IntendedUseVersion { get; init; }
    [Id(14)] public string IntendedUseName { get; init; }
    [Id(15)] public string LoanNumber { get; init; }
    [Id(16)] public DateOnly DueDate { get; init; }
}
