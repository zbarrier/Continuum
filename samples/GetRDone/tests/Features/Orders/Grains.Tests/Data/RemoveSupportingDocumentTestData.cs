using System.Security.Claims;

using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.Tests.Data;

/// <summary>
///     Supporting document removals: the commands that remove the <see cref="AttachSupportingDocumentTestData"/> documents and
///     the events they record. Each variation is removed by the user who attached it (<see cref="Command1"/> removes
///     <c>Attached1</c>, and so on). Each event is built from its command, so the two never drift apart.
/// </summary>
public static class RemoveSupportingDocumentTestData
{
    /// <summary>ClientUser1 removes the purchase contract of <see cref="AttachSupportingDocumentTestData.Command1"/>.</summary>
    public static RemoveSupportingDocumentCommand Command1 { get; } = new(AttachSupportingDocumentTestData.Command1.DocumentId);

    /// <summary>ClientUser2 removes the survey of <see cref="AttachSupportingDocumentTestData.Command2"/>.</summary>
    public static RemoveSupportingDocumentCommand Command2 { get; } = new(AttachSupportingDocumentTestData.Command2.DocumentId);

    /// <summary>ClientUser1 removes the plat photo of <see cref="AttachSupportingDocumentTestData.Command3"/>.</summary>
    public static RemoveSupportingDocumentCommand Command3 { get; } = new(AttachSupportingDocumentTestData.Command3.DocumentId);

    /// <summary>The removal <see cref="Command1"/> records, by ClientUser1.</summary>
    public static SupportingDocumentRemoved Removed1(string orderId, DateTimeOffset? occurredAt = null)
        => Removed(Command1, orderId, RequestedByUsers.ClientUser1, occurredAt);

    /// <summary>The removal <see cref="Command2"/> records, by ClientUser2.</summary>
    public static SupportingDocumentRemoved Removed2(string orderId, DateTimeOffset? occurredAt = null)
        => Removed(Command2, orderId, RequestedByUsers.ClientUser2, occurredAt);

    /// <summary>The removal <see cref="Command3"/> records, by ClientUser1.</summary>
    public static SupportingDocumentRemoved Removed3(string orderId, DateTimeOffset? occurredAt = null)
        => Removed(Command3, orderId, RequestedByUsers.ClientUser1, occurredAt);

    /// <summary>The event the order grain records when <paramref name="initiatedBy"/> removes a document with <paramref name="command"/>.</summary>
    public static SupportingDocumentRemoved Removed(
        RemoveSupportingDocumentCommand command, string orderId, ClaimsPrincipal initiatedBy, DateTimeOffset? occurredAt = null) => new()
    {
        OrderId = orderId,
        DocumentId = command.DocumentId,
        BlobPath = SupportingDocumentStorage.BlobPath(orderId, command.DocumentId),
        InitiatedBy = initiatedBy.GetTenantUserId(),
        OccurredAt = occurredAt ?? TestData.At
    };
}
