using Continuum.CSharpFunctionalExtensions;

using Orleans.Concurrency;

namespace GetRDone.Orders.Abstractions.ReadModels;

/// <summary>
///     What a client user can order: the current service request types and their form types. The sample seeds them per
///     tenant instead of projecting them from service request type events.
/// </summary>
public interface IOrderableServicesGrain : IGrainWithStringKey
{
    [ReadOnly]
    Task<OrderableService[]> GetOrderableServicesAsync();

    /// <summary>
    ///     Succeeds when the service request type and form type pair exists, both versions are current, and both names match.
    /// </summary>
    [ReadOnly]
    Task<Result> IsCurrentOrderableServiceAsync(
        ServiceRequestTypeId serviceRequestTypeId,
        ServiceRequestTypeVersion serviceRequestTypeVersion,
        ServiceRequestTypeName serviceRequestTypeName,
        FormTypeId formTypeId,
        FormTypeVersion formTypeVersion,
        FormTypeName formTypeName);
}

[GenerateSerializer, Immutable]
public sealed record OrderableService(
    string ServiceRequestTypeId,
    int ServiceRequestTypeVersion,
    string ServiceRequestTypeName,
    string FormTypeId,
    int FormTypeVersion,
    string FormTypeName);
