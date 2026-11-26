using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.Tests.Data;

/// <summary>
///     The vendor assignment: once ClientUser1 approves the description of work, Director1 assigns the order to Ridge
///     Appraisal Group, and the Vendor Assignment Notifier emails both of its users. The events follow from the commands, so
///     they never drift apart.
/// </summary>
public static class VendorAssignmentTestData
{
    /// <summary>A due date that stays in the future, since the order grain rejects a vendor due date in the past.</summary>
    public static DateOnly VendorDueDate { get; } = new(2099, 12, 4);

    /// <summary>Director1 assigns the order to Ridge Appraisal Group.</summary>
    public static AssignVendorCommand Command1 { get; } = new(TestData.RidgeAppraisalGroupVendorId, 425.00m, VendorDueDate);

    /// <summary>The event the order grain records when Director1 sends <see cref="Command1"/> for the approved first version.</summary>
    public static VendorAssigned Assigned1(string orderId, long? orderNumber = null, DateTimeOffset? occurredAt = null) => new()
    {
        OrderId = orderId,
        VendorId = Command1.VendorId,
        VendorFee = Command1.VendorFee,
        VendorDueDate = Command1.VendorDueDate,
        DescriptionOfWorkId = DescriptionOfWorkTestData.DescriptionOfWorkId,
        DescriptionOfWorkVersion = 1,
        OrderNumber = orderNumber,
        InitiatedBy = RequestedByUsers.Director1.GetTenantUserId(),
        OccurredAt = occurredAt ?? TestData.At
    };

    /// <summary>The subject the Vendor Assignment Notifier gives its notice; Director1 works for Summit Valuation.</summary>
    public static string NoticeSubject(long? orderNumber) => orderNumber is { } number
        ? $"New order {number} from Summit Valuation"
        : "A new order from Summit Valuation";

    /// <summary>Ridge Appraisal Group's users, whom the notice goes to.</summary>
    public static VendorNoticeRecipient[] Recipients { get; } =
    [
        new(TestData.VendorUser1.UserId, TestData.VendorUser1.Email),
        new(TestData.VendorUser2.UserId, TestData.VendorUser2.Email)
    ];

    /// <summary>The notice the Vendor Assignment Notifier records after emailing Ridge Appraisal Group's users.</summary>
    public static SendVendorAssignedNoticeCommand NoticeCommand(long? orderNumber = null)
        => new(TestData.RidgeAppraisalGroupVendorId, Recipients, NoticeSubject(orderNumber));

    /// <summary>The event the order grain records when the automation records <see cref="NoticeCommand"/>.</summary>
    public static VendorAssignedNoticeSent NoticeSent(string orderId, long? orderNumber = null, DateTimeOffset? occurredAt = null) => new()
    {
        OrderId = orderId,
        VendorId = TestData.RidgeAppraisalGroupVendorId,
        Recipients = Recipients,
        Subject = NoticeSubject(orderNumber),
        InitiatedBy = AutomationUser.VendorAssignmentNotifier.GetTenantUserId(TestData.TenantId),
        OccurredAt = occurredAt ?? TestData.At
    };
}
