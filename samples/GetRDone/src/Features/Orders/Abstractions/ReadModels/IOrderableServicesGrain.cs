using Orleans.Concurrency;

namespace GetRDone.Orders.Abstractions.ReadModels;

public interface IOrderableServicesGrain : IGrainWithStringKey
{
    [ReadOnly]
    Task<OrderableService[]> GetOfferingsAsync();
}

[GenerateSerializer, Immutable]
public sealed record OrderableService(
    Guid ServiceRequestTypeId,
    int ServiceRequestTypeVersion,
    string ServiceRequestTypeName,
    Guid FormTypeId,
    int FormTypeVersion,
    string FormTypeName);
