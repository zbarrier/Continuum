namespace GetRDone.Orders.Abstractions.ReadModels;

public interface IOrderableServicesGrain : IGrainWithStringKey
{
    Task<OrderableService[]> GetOfferingsAsync();
}

[GenerateSerializer, Immutable]
public sealed record OrderableService(
    [property: Id(0)] Guid ServiceRequestTypeId,
    [property: Id(1)] int ServiceRequestTypeVersion,
    [property: Id(2)] string ServiceRequestTypeName,
    [property: Id(3)] Guid FormTypeId,
    [property: Id(4)] int FormTypeVersion,
    [property: Id(5)] string FormTypeName);
