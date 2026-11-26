using Continuum.CSharpFunctionalExtensions;

using Orleans.Concurrency;

namespace GetRDone.Orders.Abstractions.ReadModels;

/// <summary>
///     The client order page of one submitted order (the ClientOrderDetails read model): its summary, the documents the AMC
///     requested, its documents, its description of work and its timeline. Each order has one instance, keyed by the order
///     ID, and only the client user who started the order can read it.
/// </summary>
public interface IClientOrderDetailsGrain : IGrainWithStringKey
{
    /// <summary>Gets the order's details; fails with not found until the order is submitted.</summary>
    [ReadOnly]
    Task<Result<ClientOrderDetails>> GetClientOrderDetailsAsync();
}

/// <summary>What the client order page shows of an order.</summary>
/// <param name="OrderId">The order.</param>
/// <param name="OrderNumber">The order number, or null until the order number assigner gives the order its number.</param>
/// <param name="Status">The order's status as the client sees it.</param>
/// <param name="FormTypeName">The form type ordered.</param>
/// <param name="PropertyAddress">The property address on one line.</param>
/// <param name="DueDate">When the client gets the report, or null until the order is assigned to an appraiser.</param>
/// <param name="ServiceFee">What the client pays the AMC, or null until the order is assigned to an appraiser.</param>
/// <param name="AssignedToName">The AMC staff member working on the order, or null until it is assigned.</param>
/// <param name="OpenRequests">The documents the AMC requested that the client has not provided yet, oldest first.</param>
/// <param name="Documents">The order's supporting documents, in the order they were added.</param>
/// <param name="DescriptionOfWork">The latest version of the order's description of work, or null until one is submitted.</param>
/// <param name="Timeline">What happened to the order, newest first.</param>
/// <param name="AllowedActions">What the client can do next.</param>
[GenerateSerializer, Immutable]
public sealed record ClientOrderDetails(
    string OrderId,
    long? OrderNumber,
    ClientOrderStatus Status,
    string FormTypeName,
    string PropertyAddress,
    DateOnly? DueDate,
    decimal? ServiceFee,
    string? AssignedToName,
    ClientDocumentRequest[] OpenRequests,
    ClientOrderDocument[] Documents,
    DescriptionOfWorkSummary? DescriptionOfWork,
    OrderTimelineEntry[] Timeline,
    ClientOrderActions AllowedActions);

/// <summary>Documents the AMC requested from the client.</summary>
/// <param name="RequestId">The request, which the client names when it provides the document.</param>
/// <param name="DocumentTypeName">The type of document requested, such as <c>Rent roll</c>.</param>
/// <param name="Message">What the AMC told the client about it.</param>
/// <param name="RequestedAt">When it was requested.</param>
[GenerateSerializer, Immutable]
public sealed record ClientDocumentRequest(
    string RequestId,
    string DocumentTypeName,
    string Message,
    DateTimeOffset RequestedAt);

/// <summary>A supporting document of the order.</summary>
/// <param name="DocumentId">The document.</param>
/// <param name="FileName">The name of its file.</param>
/// <param name="DocumentTypeName">What kind of document it is.</param>
[GenerateSerializer, Immutable]
public sealed record ClientOrderDocument(
    string DocumentId,
    string FileName,
    string DocumentTypeName);

/// <summary>What the client can do next on the client order page.</summary>
[Flags]
public enum ClientOrderActions
{
    None = 0,

    /// <summary>Upload a document the AMC requested.</summary>
    UploadRequestedDocument = 1,

    /// <summary>Approve the description of work that waits for their approval.</summary>
    ApproveDescriptionOfWork = 2
}
