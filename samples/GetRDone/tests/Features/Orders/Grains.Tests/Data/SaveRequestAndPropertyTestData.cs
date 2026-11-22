using System.Security.Claims;

using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.Tests.Data;

/// <summary>
///     Request &amp; Property tab variations saved after the draft was started: the commands that save them and the events
///     they record, for the Given of any test that needs a changed tab. Each variation is saved by the user who started the
///     matching <see cref="StartOrderDraftTestData"/> draft (<see cref="Command1"/> on <c>DraftStarted1</c>, and so on). Each
///     event is built from its command, so the two never drift apart.
/// </summary>
public static class SaveRequestAndPropertyTestData
{
    /// <summary>ClientUser1 corrects the street and the loan number of draft 1 and moves its due date.</summary>
    public static SaveRequestAndPropertyCommand Command1 { get; } = From(StartOrderDraftTestData.Command1) with
    {
        PropertyStreet = "84 Birch Hollow Rd",
        LoanNumber = "LCU-2026-1178",
        DueDate = new DateOnly(2026, 12, 15)
    };

    /// <summary>ClientUser2 switches draft 2 to a narrative report for a refinance.</summary>
    public static SaveRequestAndPropertyCommand Command2 { get; } = From(StartOrderDraftTestData.Command2) with
    {
        FormTypeId = TestData.NarrativeAppraisal.FormTypeId,
        FormTypeVersion = TestData.NarrativeAppraisal.FormTypeVersion,
        FormTypeName = TestData.NarrativeAppraisal.FormTypeName,
        IntendedUseId = TestData.Refinance.IntendedUseId,
        IntendedUseVersion = TestData.Refinance.IntendedUseVersion,
        IntendedUseName = TestData.Refinance.Name
    };

    /// <summary>ClientUser1 switches draft 3 to a full residential appraisal of a townhouse.</summary>
    public static SaveRequestAndPropertyCommand Command3 { get; } = From(StartOrderDraftTestData.Command3) with
    {
        FormTypeId = TestData.UniformResidentialAppraisal.FormTypeId,
        FormTypeVersion = TestData.UniformResidentialAppraisal.FormTypeVersion,
        FormTypeName = TestData.UniformResidentialAppraisal.FormTypeName,
        PropertyType = "Townhouse"
    };

    /// <summary>The tab <see cref="Command1"/> saves, by ClientUser1.</summary>
    public static RequestAndPropertySaved Saved1(string orderId, DateTimeOffset? occurredAt = null)
        => Saved(Command1, orderId, RequestedByUsers.ClientUser1, occurredAt);

    /// <summary>The tab <see cref="Command2"/> saves, by ClientUser2.</summary>
    public static RequestAndPropertySaved Saved2(string orderId, DateTimeOffset? occurredAt = null)
        => Saved(Command2, orderId, RequestedByUsers.ClientUser2, occurredAt);

    /// <summary>The tab <see cref="Command3"/> saves, by ClientUser1.</summary>
    public static RequestAndPropertySaved Saved3(string orderId, DateTimeOffset? occurredAt = null)
        => Saved(Command3, orderId, RequestedByUsers.ClientUser1, occurredAt);

    /// <summary>
    ///     The event the order grain records when <paramref name="initiatedBy"/> saves the tab with <paramref name="command"/>;
    ///     use it directly for a one-off variation, such as <c>Command1 with { DueDate = ... }</c>.
    /// </summary>
    public static RequestAndPropertySaved Saved(
        SaveRequestAndPropertyCommand command, string orderId, ClaimsPrincipal initiatedBy, DateTimeOffset? occurredAt = null) => new()
    {
        OrderId = orderId,
        ServiceRequestTypeId = command.ServiceRequestTypeId,
        ServiceRequestTypeVersion = command.ServiceRequestTypeVersion,
        ServiceRequestTypeName = command.ServiceRequestTypeName,
        FormTypeId = command.FormTypeId,
        FormTypeVersion = command.FormTypeVersion,
        FormTypeName = command.FormTypeName,
        PropertyStreet = command.PropertyStreet,
        PropertyCity = command.PropertyCity,
        PropertyState = command.PropertyState,
        PropertyZip = command.PropertyZip,
        PropertyType = command.PropertyType,
        IntendedUseId = command.IntendedUseId,
        IntendedUseVersion = command.IntendedUseVersion,
        IntendedUseName = command.IntendedUseName,
        LoanNumber = command.LoanNumber,
        DueDate = command.DueDate,
        InitiatedBy = initiatedBy.GetTenantUserId(),
        OccurredAt = occurredAt ?? TestData.At
    };

    /// <summary>The tab as <paramref name="start"/> saved it, to change from.</summary>
    private static SaveRequestAndPropertyCommand From(StartOrderDraftCommand start) => new(
        start.ServiceRequestTypeId, start.ServiceRequestTypeVersion, start.ServiceRequestTypeName,
        start.FormTypeId, start.FormTypeVersion, start.FormTypeName,
        start.PropertyStreet, start.PropertyCity, start.PropertyState, start.PropertyZip, start.PropertyType,
        start.IntendedUseId, start.IntendedUseVersion, start.IntendedUseName,
        start.LoanNumber, start.DueDate);
}
