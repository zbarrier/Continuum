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

/// <summary>The saved tabs of a draft. The Borrower &amp; Access contact values are <see langword="null"/> until that tab is saved.</summary>
/// <param name="OrderId">The order the draft belongs to.</param>
/// <param name="ServiceRequestTypeName">The service ordered.</param>
/// <param name="FormTypeName">The form type ordered.</param>
/// <param name="PropertyAddress">The property address on one line.</param>
/// <param name="PropertyType">The type of the property.</param>
/// <param name="IntendedUseName">The intended use of the appraisal.</param>
/// <param name="LoanNumber">The client's loan number.</param>
/// <param name="DueDate">When the appraisal is due.</param>
/// <param name="BorrowerName">The borrower.</param>
/// <param name="AccessContactName">The person who gives access to the property, if any.</param>
/// <param name="AccessContactPhone">The access contact's 10-digit phone number, digits only.</param>
/// <param name="LastSavedAt">When the draft was last saved.</param>
[GenerateSerializer, Immutable]
public sealed record OrderDraft(
    string OrderId,
    string ServiceRequestTypeName,
    string FormTypeName,
    string PropertyAddress,
    string PropertyType,
    string IntendedUseName,
    string LoanNumber,
    DateOnly DueDate,
    string? BorrowerName,
    string? AccessContactName,
    string? AccessContactPhone,
    DateTimeOffset LastSavedAt);
