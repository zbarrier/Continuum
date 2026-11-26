using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.Tests.Data;

/// <summary>
///     The description of work: Director1, who <c>OrderAssigned1</c> assigns the order to, submits it for the approval of
///     ClientUser1, who started <c>DraftStarted1</c>, then revises it. The approval and the notices follow from the
///     submissions, so they never drift apart.
/// </summary>
public static class DescriptionOfWorkTestData
{
    public static string DescriptionOfWorkId { get; } = "summit|5c2e8a17-9d4b-4f63-a0e1-7b3c9d2f6a58";

    /// <summary>Director1 submits the first version of the description of work, with a bullet list as Quill 2 writes it.</summary>
    public static SubmitDescriptionOfWorkCommand Command1 { get; } = new(
        DescriptionOfWorkId, 1,
        "<p>Appraise the <strong>interior and exterior</strong> of 48 Birch Hollow Rd.</p>"
        + "<ol><li data-list=\"bullet\"><span class=\"ql-ui\"></span>Interior photos</li>"
        + "<li data-list=\"bullet\" class=\"ql-indent-1\"><span class=\"ql-ui\"></span>Three comparable sales</li></ol>");

    /// <summary>Director1 revises <see cref="Command1"/> before ClientUser1 approves it.</summary>
    public static SubmitDescriptionOfWorkCommand Command2 { get; } = new(
        DescriptionOfWorkId, 2,
        "<p>Appraise the <strong>interior and exterior</strong> of 48 Birch Hollow Rd, including the detached garage.</p>");

    /// <summary>ClientUser1 approves the first version.</summary>
    public static ApproveDescriptionOfWorkCommand ApproveCommand1 { get; } = new(DescriptionOfWorkId, 1);

    /// <summary>The event the order grain records when Director1 sends <see cref="Command1"/>.</summary>
    public static DescriptionOfWorkSubmitted Submitted1(string orderId, long? orderNumber = null, DateTimeOffset? occurredAt = null)
        => Submitted(Command1, orderId, orderNumber, occurredAt);

    /// <summary>The event the order grain records when Director1 sends <paramref name="command"/> for ClientUser1's order.</summary>
    public static DescriptionOfWorkSubmitted Submitted(
        SubmitDescriptionOfWorkCommand command, string orderId, long? orderNumber = null, DateTimeOffset? occurredAt = null) => new()
    {
        OrderId = orderId,
        DescriptionOfWorkId = command.DescriptionOfWorkId,
        DescriptionOfWorkVersion = command.DescriptionOfWorkVersion,
        DescriptionOfWorkHtml = command.DescriptionOfWorkHtml,
        ApproverUserId = RequestedByUsers.ClientUser1.GetTenantUserId(),
        OrderNumber = orderNumber,
        InitiatedBy = RequestedByUsers.Director1.GetTenantUserId(),
        OccurredAt = occurredAt ?? TestData.At
    };

    /// <summary>The event the order grain records when ClientUser1 approves <paramref name="version"/>.</summary>
    public static DescriptionOfWorkApproved Approved(
        string orderId, int version = 1, long? orderNumber = null, DateTimeOffset? occurredAt = null) => new()
    {
        OrderId = orderId,
        DescriptionOfWorkId = DescriptionOfWorkId,
        DescriptionOfWorkVersion = version,
        AssignedToUserId = RequestedByUsers.Director1.GetTenantUserId(),
        OrderNumber = orderNumber,
        InitiatedBy = RequestedByUsers.ClientUser1.GetTenantUserId(),
        OccurredAt = occurredAt ?? TestData.At
    };

    /// <summary>The subject the Description of Work Approval Notifier gives the notice of a submitted version.</summary>
    public static string SubmittedNoticeSubject(long? orderNumber) => orderNumber is { } number
        ? $"Order {number}: please approve the description of work"
        : "Your order: please approve the description of work";

    /// <summary>The notice the Description of Work Approval Notifier records after emailing ClientUser1 about <paramref name="version"/>.</summary>
    public static SendDescriptionOfWorkSubmittedNoticeCommand SubmittedNoticeCommand(int version = 1, long? orderNumber = null) => new(
        DescriptionOfWorkId, version, TestData.ClientUser1.UserId, TestData.ClientUser1.Email, SubmittedNoticeSubject(orderNumber));

    /// <summary>The event the order grain records when the automation records <see cref="SubmittedNoticeCommand"/>.</summary>
    public static DescriptionOfWorkSubmittedNoticeSent SubmittedNoticeSent(
        string orderId, int version = 1, long? orderNumber = null, DateTimeOffset? occurredAt = null) => new()
    {
        OrderId = orderId,
        DescriptionOfWorkId = DescriptionOfWorkId,
        DescriptionOfWorkVersion = version,
        RecipientUserId = TestData.ClientUser1.UserId,
        RecipientEmail = TestData.ClientUser1.Email,
        Subject = SubmittedNoticeSubject(orderNumber),
        InitiatedBy = AutomationUser.DescriptionOfWorkApprovalNotifier.GetTenantUserId(TestData.TenantId),
        OccurredAt = occurredAt ?? TestData.At
    };

    /// <summary>The subject the Description of Work Approved Notifier gives its notice.</summary>
    public static string ApprovedNoticeSubject(long? orderNumber) => orderNumber is { } number
        ? $"Order {number}: the description of work was approved"
        : "An order's description of work was approved";

    /// <summary>The notice the Description of Work Approved Notifier records after emailing Director1.</summary>
    public static SendDescriptionOfWorkApprovedNoticeCommand ApprovedNoticeCommand(long? orderNumber = null) => new(
        DescriptionOfWorkId, TestData.Director1.UserId, TestData.Director1.Email, ApprovedNoticeSubject(orderNumber));

    /// <summary>The event the order grain records when the automation records <see cref="ApprovedNoticeCommand"/>.</summary>
    public static DescriptionOfWorkApprovedNoticeSent ApprovedNoticeSent(
        string orderId, long? orderNumber = null, DateTimeOffset? occurredAt = null) => new()
    {
        OrderId = orderId,
        DescriptionOfWorkId = DescriptionOfWorkId,
        RecipientUserId = TestData.Director1.UserId,
        RecipientEmail = TestData.Director1.Email,
        Subject = ApprovedNoticeSubject(orderNumber),
        InitiatedBy = AutomationUser.DescriptionOfWorkApprovedNotifier.GetTenantUserId(TestData.TenantId),
        OccurredAt = occurredAt ?? TestData.At
    };
}
