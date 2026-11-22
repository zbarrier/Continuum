using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain.Orleans;

using Orleans.Concurrency;

namespace GetRDone.Orders.Abstractions.OrderAggregate;

public partial interface IOrderGrain : IEventDrivenStateMachineGrain
{
    /// <summary>
    ///     Gets the order entry form of the draft (the OrderDraft read model). The order builds it from its own events, so
    ///     the tabs always show the latest save. Only the user who started the draft can read it.
    /// </summary>
    [ReadOnly]
    Task<Result<OrderDraft>> GetOrderDraftAsync();
}

/// <summary>
///     The saved tabs of a draft, with the IDs and versions of the service, form type and intended use, so the Request
///     &amp; Property tab can show them for editing. The Borrower &amp; Access contact values are <see langword="null"/>
///     until that tab is saved.
/// </summary>
/// <param name="OrderId">The order the draft belongs to.</param>
/// <param name="ServiceRequestTypeId">The service ordered.</param>
/// <param name="ServiceRequestTypeVersion">The version of the service ordered.</param>
/// <param name="ServiceRequestTypeName">The name of the service ordered.</param>
/// <param name="FormTypeId">The form type ordered.</param>
/// <param name="FormTypeVersion">The version of the form type ordered.</param>
/// <param name="FormTypeName">The name of the form type ordered.</param>
/// <param name="PropertyStreet">The street of the property.</param>
/// <param name="PropertyCity">The city of the property.</param>
/// <param name="PropertyState">The two-letter state of the property.</param>
/// <param name="PropertyZip">The ZIP code of the property.</param>
/// <param name="PropertyType">The type of the property.</param>
/// <param name="IntendedUseId">The intended use of the appraisal.</param>
/// <param name="IntendedUseVersion">The version of the intended use.</param>
/// <param name="IntendedUseName">The name of the intended use.</param>
/// <param name="LoanNumber">The client's loan number.</param>
/// <param name="DueDate">When the appraisal is due.</param>
/// <param name="BorrowerName">The borrower.</param>
/// <param name="AccessContactName">The person who gives access to the property, if any.</param>
/// <param name="AccessContactPhone">The access contact's 10-digit phone number, digits only.</param>
/// <param name="LastSavedAt">When the draft was last saved.</param>
[GenerateSerializer, Immutable]
public sealed record OrderDraft(
    string OrderId,
    string ServiceRequestTypeId,
    int ServiceRequestTypeVersion,
    string ServiceRequestTypeName,
    string FormTypeId,
    int FormTypeVersion,
    string FormTypeName,
    string PropertyStreet,
    string PropertyCity,
    string PropertyState,
    string PropertyZip,
    string PropertyType,
    string IntendedUseId,
    int IntendedUseVersion,
    string IntendedUseName,
    string LoanNumber,
    DateOnly DueDate,
    string? BorrowerName,
    string? AccessContactName,
    string? AccessContactPhone,
    DateTimeOffset LastSavedAt)
{
    /// <summary>The property address on one line.</summary>
    public string PropertyAddress => $"{PropertyStreet}, {PropertyCity}, {PropertyState} {PropertyZip}";
}
