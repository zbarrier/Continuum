using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.Tests.Data;

/// <summary>
///     The vendor's work: once Director1 assigns the order to Ridge Appraisal Group, VendorUser1 schedules the inspection
///     with the access contact, completes it and delivers the report, and the Report Delivered Notifier emails Director1,
///     whom the order is assigned to. The events follow from the commands, so they never drift apart.
/// </summary>
public static class InspectionTestData
{
    /// <summary>An inspection time that stays in the future, since the order grain rejects one in the past.</summary>
    public static DateTimeOffset InspectionTime { get; } = new(2099, 11, 30, 10, 0, 0, TimeSpan.FromHours(-5));

    /// <summary>VendorUser1 schedules the inspection with Marcus Doyle, the access contact <c>Saved1</c> names.</summary>
    public static ScheduleInspectionCommand ScheduleCommand1 { get; } = new(InspectionTime, "Marcus Doyle");

    /// <summary>VendorUser1 records the inspection on a date that is already past, since the order grain rejects a future one.</summary>
    public static CompleteInspectionCommand CompleteCommand1 { get; } = new(new DateOnly(2026, 11, 17));

    /// <summary>VendorUser1 delivers the report of 48 Birch Hollow Rd.</summary>
    public static DeliverReportCommand DeliverCommand1 { get; } = new(
        "summit|8f2c4e6a-1b3d-4f5a-9c7e-0d2b4a6c8e13",
        "Appraisal_48BirchHollow.pdf", "application/pdf", 3_482_117);

    /// <summary>The event the order grain records when VendorUser1 sends <see cref="ScheduleCommand1"/>.</summary>
    public static InspectionScheduled Scheduled1(string orderId, DateTimeOffset? occurredAt = null) => new()
    {
        OrderId = orderId,
        InspectionAt = ScheduleCommand1.InspectionAt,
        ContactName = ScheduleCommand1.ContactName,
        InitiatedBy = RequestedByUsers.VendorUser1.GetTenantUserId(),
        OccurredAt = occurredAt ?? TestData.At
    };

    /// <summary>The event the order grain records when VendorUser1 sends <see cref="CompleteCommand1"/>.</summary>
    public static InspectionCompleted Completed1(string orderId, DateTimeOffset? occurredAt = null) => new()
    {
        OrderId = orderId,
        CompletedOn = CompleteCommand1.CompletedOn,
        InitiatedBy = RequestedByUsers.VendorUser1.GetTenantUserId(),
        OccurredAt = occurredAt ?? TestData.At
    };

    /// <summary>The event the order grain records when VendorUser1 sends <see cref="DeliverCommand1"/> for Director1's order.</summary>
    public static ReportDelivered Delivered1(string orderId, long? orderNumber = null, DateTimeOffset? occurredAt = null) => new()
    {
        OrderId = orderId,
        DocumentId = DeliverCommand1.DocumentId,
        FileName = DeliverCommand1.FileName,
        ContentType = DeliverCommand1.ContentType,
        FileSize = DeliverCommand1.FileSize,
        BlobPath = SupportingDocumentStorage.BlobPath(orderId, DeliverCommand1.DocumentId),
        AssignedToUserId = RequestedByUsers.Director1.GetTenantUserId(),
        OrderNumber = orderNumber,
        InitiatedBy = RequestedByUsers.VendorUser1.GetTenantUserId(),
        OccurredAt = occurredAt ?? TestData.At
    };

    /// <summary>The subject the Report Delivered Notifier gives its notice.</summary>
    public static string NoticeSubject(long? orderNumber) => orderNumber is { } number
        ? $"Order {number}: the report was delivered"
        : "An order's report was delivered";

    /// <summary>The notice the Report Delivered Notifier records after emailing Director1.</summary>
    public static SendReportDeliveredNoticeCommand NoticeCommand1(long? orderNumber = null) => new(
        DeliverCommand1.DocumentId,
        TestData.Director1.UserId,
        TestData.Director1.Email,
        NoticeSubject(orderNumber));

    /// <summary>The event the order grain records when the automation records <see cref="NoticeCommand1"/>.</summary>
    public static ReportDeliveredNoticeSent NoticeSent1(string orderId, long? orderNumber = null, DateTimeOffset? occurredAt = null) => new()
    {
        OrderId = orderId,
        DocumentId = DeliverCommand1.DocumentId,
        RecipientUserId = TestData.Director1.UserId,
        RecipientEmail = TestData.Director1.Email,
        Subject = NoticeSubject(orderNumber),
        InitiatedBy = AutomationUser.ReportDeliveredNotifier.GetTenantUserId(TestData.TenantId),
        OccurredAt = occurredAt ?? TestData.At
    };
}
