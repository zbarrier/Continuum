using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

namespace GetRDone.Orders.Abstractions.OrderAggregate;

public partial interface IOrderGrain : IEventDrivenStateMachineGrain
{
    Task<Result<SaveBorrowerAndAccessContactResponse>> SaveBorrowerAndAccessContactAsync(SaveBorrowerAndAccessContactCommand command);
}

/// <summary>
///     Saves the Borrower &amp; Access contact tab of a draft. The borrower is required. The access contact is optional, but
///     its name and phone go together: when either is given, both are required.
/// </summary>
[Alias("Orders.SaveBorrowerAndAccessContactCommand.V1"), GenerateSerializer, Immutable]
public sealed record SaveBorrowerAndAccessContactCommand(
    string BorrowerName,
    string? AccessContactName,
    string? AccessContactPhone);

[Alias("Orders.SaveBorrowerAndAccessContactResponse.V1"), GenerateSerializer, Immutable]
public sealed class SaveBorrowerAndAccessContactResponse(IReadOnlyList<IChange> changes, int version) : CommandResponseWithChangesBase(changes, version);

/// <summary>
///     The borrower and the person who gives access to the property (form data, not users). The access contact's name and
///     phone are either both set or both <see langword="null"/>.
/// </summary>
[Alias("Orders.BorrowerAndAccessContactSaved.V1"), DomainEventType("Orders.BorrowerAndAccessContactSaved.V1"), GenerateSerializer, Immutable]
public sealed class BorrowerAndAccessContactSaved : OrderEvent
{
    [Id(0)] public required string OrderId { get; init; }
    [Id(1)] public required string BorrowerName { get; init; }
    [Id(2)] public string? AccessContactName { get; init; }

    /// <summary>The access contact's 10-digit US phone number, digits only.</summary>
    [Id(3)] public string? AccessContactPhone { get; init; }
}
