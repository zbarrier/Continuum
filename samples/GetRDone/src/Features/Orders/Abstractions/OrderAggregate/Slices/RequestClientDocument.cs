using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

namespace GetRDone.Orders.Abstractions.OrderAggregate;

public partial interface IOrderGrain : IEventDrivenStateMachineGrain
{
    /// <summary>
    ///     Asks the client who started the order for a document of a type, such as a rent roll, with a message. The staff
    ///     member the order is assigned to, or anyone who may assign orders, may ask once the order is acknowledged; the
    ///     Document Request Notifier then emails the client.
    /// </summary>
    Task<Result<RequestClientDocumentResponse>> RequestClientDocumentAsync(RequestClientDocumentCommand command);
}

/// <summary>The document the AMC asks the client for. The requester's client generates the request ID.</summary>
[Alias("Orders.RequestClientDocumentCommand.V1"), GenerateSerializer, Immutable]
public sealed record RequestClientDocumentCommand(
    string RequestId,
    string DocumentTypeId,
    int DocumentTypeVersion,
    string DocumentTypeName,
    string Message);

[Alias("Orders.RequestClientDocumentResponse.V1"), GenerateSerializer, Immutable]
public sealed class RequestClientDocumentResponse(IReadOnlyList<IChange> changes, int version) : CommandResponseWithChangesBase(changes, version);

/// <summary>The AMC asked the client who started the order for a document of a type.</summary>
/// <remarks>
///     It carries the order's number, when it has one, so the Document Request Notifier can name the order in its email
///     without reading the order.
/// </remarks>
[Alias("Orders.ClientDocumentRequested.V1"), DomainEventType("Orders.ClientDocumentRequested.V1"), GenerateSerializer, Immutable]
public sealed class ClientDocumentRequested : OrderEvent
{
    [Id(0)] public required string OrderId { get; init; }
    [Id(1)] public required string RequestId { get; init; }

    /// <summary>The client user who started the order, who is asked for the document.</summary>
    [Id(2)] public required string RequestedFromUserId { get; init; }

    [Id(3)] public required string DocumentTypeId { get; init; }
    [Id(4)] public required int DocumentTypeVersion { get; init; }
    [Id(5)] public required string DocumentTypeName { get; init; }
    [Id(6)] public required string Message { get; init; }
    [Id(7)] public long? OrderNumber { get; init; }
}
