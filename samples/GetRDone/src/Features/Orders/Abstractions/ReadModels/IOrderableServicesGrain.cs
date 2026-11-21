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
    ///     Succeeds when the service request type exists and the form type is one of its form types, with both versions
    ///     current and both names matching.
    /// </summary>
    [ReadOnly]
    Task<Result> IsCurrentOrderableServiceAsync(ServiceRequestTypeRef serviceRequestType, FormTypeRef formType);
}

/// <summary>A service request type and the form types that can be ordered with it.</summary>
[GenerateSerializer, Immutable]
public sealed record OrderableService(
    string ServiceRequestTypeId,
    int ServiceRequestTypeVersion,
    string ServiceRequestTypeName,
    OrderableFormType[] FormTypes);

[GenerateSerializer, Immutable]
public sealed record OrderableFormType(string FormTypeId, int FormTypeVersion, string FormTypeName);
