using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.Tests.Data;

/// <summary>
///     Document requests: Director1, who <c>OrderAssigned1</c> assigns the order to, asks ClientUser1, who started
///     <c>DraftStarted1</c>, for the property's survey. The notice and the provided document follow from the request, so
///     the three never drift apart.
/// </summary>
public static class DocumentRequestTestData
{
    /// <summary>Director1 asks for the survey of 48 Birch Hollow Rd.</summary>
    public static RequestClientDocumentCommand Command1 { get; } = new(
        "summit|e1a7c3d9-5b2f-4c86-9a0e-7d3f1b6c8a42",
        TestData.Survey.DocumentTypeId, TestData.Survey.DocumentTypeVersion, TestData.Survey.Name,
        "Please upload the most recent survey of the property; the lender needs it before the inspection.");

    /// <summary>Director1 also asks for the purchase contract of 48 Birch Hollow Rd.</summary>
    public static RequestClientDocumentCommand Command2 { get; } = new(
        "summit|6b0d4f82-1e9a-4c37-8d5b-a2c7e3f90d18",
        TestData.PurchaseContract.DocumentTypeId, TestData.PurchaseContract.DocumentTypeVersion, TestData.PurchaseContract.Name,
        "Please upload the signed purchase contract.");

    /// <summary>ClientUser1 provides the survey <see cref="Command1"/> asks for.</summary>
    public static ProvideRequestedDocumentCommand ProvideCommand1 { get; } = new(
        Command1.RequestId,
        "summit|3d9f6b21-8c4e-4a57-b0d3-5e2a7c9f1b64",
        "Survey_48BirchHollow.pdf", "application/pdf", 1_204_551);

    /// <summary>The event the order grain records when Director1 sends <see cref="Command1"/>.</summary>
    public static ClientDocumentRequested Requested1(string orderId, long? orderNumber = null, DateTimeOffset? occurredAt = null)
        => Requested(Command1, orderId, orderNumber, occurredAt);

    /// <summary>The event the order grain records when Director1 sends <paramref name="command"/> for ClientUser1's order.</summary>
    public static ClientDocumentRequested Requested(
        RequestClientDocumentCommand command, string orderId, long? orderNumber = null, DateTimeOffset? occurredAt = null) => new()
    {
        OrderId = orderId,
        RequestId = command.RequestId,
        RequestedFromUserId = RequestedByUsers.ClientUser1.GetTenantUserId(),
        DocumentTypeId = command.DocumentTypeId,
        DocumentTypeVersion = command.DocumentTypeVersion,
        DocumentTypeName = command.DocumentTypeName,
        Message = command.Message,
        OrderNumber = orderNumber,
        InitiatedBy = RequestedByUsers.Director1.GetTenantUserId(),
        OccurredAt = occurredAt ?? TestData.At
    };

    /// <summary>The subject the Document Request Notifier gives the notice of <see cref="Command1"/>.</summary>
    public static string Subject(long? orderNumber) => orderNumber is { } number
        ? $"Order {number}: please upload a Survey"
        : "Your order: please upload a Survey";

    /// <summary>The notice the Document Request Notifier records after emailing ClientUser1 about <see cref="Command1"/>.</summary>
    public static SendDocumentRequestNoticeCommand NoticeCommand1(long? orderNumber = null) => new(
        Command1.RequestId,
        TestData.ClientUser1.UserId,
        TestData.ClientUser1.Email,
        Subject(orderNumber));

    /// <summary>The event the order grain records when the automation records <see cref="NoticeCommand1"/>.</summary>
    public static DocumentRequestNoticeSent NoticeSent1(string orderId, long? orderNumber = null, DateTimeOffset? occurredAt = null) => new()
    {
        OrderId = orderId,
        RequestId = Command1.RequestId,
        RecipientUserId = TestData.ClientUser1.UserId,
        RecipientEmail = TestData.ClientUser1.Email,
        Subject = Subject(orderNumber),
        InitiatedBy = AutomationUser.DocumentRequestNotifier.GetTenantUserId(TestData.TenantId),
        OccurredAt = occurredAt ?? TestData.At
    };

    /// <summary>The event the order grain records when ClientUser1 sends <see cref="ProvideCommand1"/>.</summary>
    public static RequestedDocumentProvided Provided1(string orderId, DateTimeOffset? occurredAt = null) => new()
    {
        OrderId = orderId,
        RequestId = ProvideCommand1.RequestId,
        DocumentId = ProvideCommand1.DocumentId,
        DocumentTypeId = Command1.DocumentTypeId,
        DocumentTypeVersion = Command1.DocumentTypeVersion,
        DocumentTypeName = Command1.DocumentTypeName,
        FileName = ProvideCommand1.FileName,
        ContentType = ProvideCommand1.ContentType,
        FileSize = ProvideCommand1.FileSize,
        BlobPath = SupportingDocumentStorage.BlobPath(orderId, ProvideCommand1.DocumentId),
        InitiatedBy = RequestedByUsers.ClientUser1.GetTenantUserId(),
        OccurredAt = occurredAt ?? TestData.At
    };
}
