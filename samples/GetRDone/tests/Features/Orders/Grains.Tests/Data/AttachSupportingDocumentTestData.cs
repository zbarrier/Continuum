using System.Security.Claims;

using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.Tests.Data;

/// <summary>
///     Supporting document variations: the commands that attach them and the events they record, for the Given of any test
///     that needs an attached document. Each variation is attached by the user who started the matching
///     <see cref="StartOrderDraftTestData"/> draft (<see cref="Command1"/> on <c>DraftStarted1</c>, and so on). Each event is
///     built from its command, so the two never drift apart.
/// </summary>
public static class AttachSupportingDocumentTestData
{
    /// <summary>ClientUser1 attaches the signed purchase contract of 48 Birch Hollow Rd, with a description.</summary>
    public static AttachSupportingDocumentCommand Command1 { get; } = new(
        "summit|0f4b8d2e-6a1c-4e3f-9b7d-2c5e8a1f3d60",
        TestData.PurchaseContract.DocumentTypeId, TestData.PurchaseContract.DocumentTypeVersion, TestData.PurchaseContract.Name,
        "Purchase_Contract_48BirchHollow.pdf", "application/pdf", 482_133,
        "Signed purchase contract with both addenda");

    /// <summary>ClientUser2 attaches the survey of 715 Commerce Blvd, without a description.</summary>
    public static AttachSupportingDocumentCommand Command2 { get; } = new(
        "summit|7c2e9a14-3b5d-4f60-8e1a-d4b7c0f2a938",
        TestData.Survey.DocumentTypeId, TestData.Survey.DocumentTypeVersion, TestData.Survey.Name,
        "Survey_715CommerceBlvd.pdf", "application/pdf", 2_381_904,
        null);

    /// <summary>ClientUser1 attaches a photo of the plat for 9 Laurel Ridge Ct as its survey.</summary>
    public static AttachSupportingDocumentCommand Command3 { get; } = new(
        "summit|b5d1f73a-9e2c-4a8b-a6f0-1e3c5d7b9f24",
        TestData.Survey.DocumentTypeId, TestData.Survey.DocumentTypeVersion, TestData.Survey.Name,
        "Plat_9LaurelRidge.jpg", "image/jpeg", 3_145_728,
        "Recorded plat (photo)");

    /// <summary>The document <see cref="Command1"/> attaches, by ClientUser1.</summary>
    public static SupportingDocumentAttached Attached1(string orderId, DateTimeOffset? occurredAt = null)
        => Attached(Command1, orderId, RequestedByUsers.ClientUser1, occurredAt);

    /// <summary>The document <see cref="Command2"/> attaches, by ClientUser2.</summary>
    public static SupportingDocumentAttached Attached2(string orderId, DateTimeOffset? occurredAt = null)
        => Attached(Command2, orderId, RequestedByUsers.ClientUser2, occurredAt);

    /// <summary>The document <see cref="Command3"/> attaches, by ClientUser1.</summary>
    public static SupportingDocumentAttached Attached3(string orderId, DateTimeOffset? occurredAt = null)
        => Attached(Command3, orderId, RequestedByUsers.ClientUser1, occurredAt);

    /// <summary>
    ///     The event the order grain records when <paramref name="initiatedBy"/> attaches the document with
    ///     <paramref name="command"/>; use it directly for a one-off variation, such as <c>Command1 with { Description = ... }</c>.
    /// </summary>
    public static SupportingDocumentAttached Attached(
        AttachSupportingDocumentCommand command, string orderId, ClaimsPrincipal initiatedBy, DateTimeOffset? occurredAt = null) => new()
    {
        OrderId = orderId,
        DocumentId = command.DocumentId,
        DocumentTypeId = command.DocumentTypeId,
        DocumentTypeVersion = command.DocumentTypeVersion,
        DocumentTypeName = command.DocumentTypeName,
        FileName = command.FileName,
        ContentType = command.ContentType,
        FileSize = command.FileSize,
        Description = command.Description,
        BlobPath = SupportingDocumentStorage.BlobPath(orderId, command.DocumentId),
        InitiatedBy = initiatedBy.GetTenantUserId(),
        OccurredAt = occurredAt ?? TestData.At
    };
}
