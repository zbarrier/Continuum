using System.Security.Claims;

using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.Tests.Data;

/// <summary>
///     Order draft variations built from <see cref="TestData"/>: the commands that start them and the events they record,
///     for the Given of any test that needs a started order. Each event is built from its command, so the two never drift
///     apart.
/// </summary>
public static class StartOrderDraftTestData
{
    /// <summary>ClientUser1 orders a full residential appraisal for Lakeview Credit Union, for a refinance.</summary>
    public static StartOrderDraftCommand Command1 { get; } = new(
        TestData.LakeviewCreditUnionClientId,
        TestData.ResidentialAppraisal.ServiceRequestTypeId, TestData.ResidentialAppraisal.ServiceRequestTypeVersion,
        TestData.ResidentialAppraisal.ServiceRequestTypeName,
        TestData.UniformResidentialAppraisal.FormTypeId, TestData.UniformResidentialAppraisal.FormTypeVersion,
        TestData.UniformResidentialAppraisal.FormTypeName,
        "48 Birch Hollow Rd", "Asheville", "NC", "28803", "Single Family",
        TestData.Refinance.IntendedUseId, TestData.Refinance.IntendedUseVersion, TestData.Refinance.Name,
        "LCU-2026-1187");

    /// <summary>ClientUser2 orders a restricted commercial appraisal for Harbor National Bank, for a purchase.</summary>
    public static StartOrderDraftCommand Command2 { get; } = new(
        TestData.HarborNationalBankClientId,
        TestData.CommercialAppraisal.ServiceRequestTypeId, TestData.CommercialAppraisal.ServiceRequestTypeVersion,
        TestData.CommercialAppraisal.ServiceRequestTypeName,
        TestData.RestrictedAppraisal.FormTypeId, TestData.RestrictedAppraisal.FormTypeVersion,
        TestData.RestrictedAppraisal.FormTypeName,
        "715 Commerce Blvd", "Greenville", "SC", "29601", "Office",
        TestData.Purchase.IntendedUseId, TestData.Purchase.IntendedUseVersion, TestData.Purchase.Name,
        "HNB-2026-0442");

    /// <summary>ClientUser1 orders a desktop residential appraisal of another property for Lakeview Credit Union, for a purchase.</summary>
    public static StartOrderDraftCommand Command3 { get; } = new(
        TestData.LakeviewCreditUnionClientId,
        TestData.ResidentialAppraisal.ServiceRequestTypeId, TestData.ResidentialAppraisal.ServiceRequestTypeVersion,
        TestData.ResidentialAppraisal.ServiceRequestTypeName,
        TestData.DesktopAppraisal.FormTypeId, TestData.DesktopAppraisal.FormTypeVersion,
        TestData.DesktopAppraisal.FormTypeName,
        "9 Laurel Ridge Ct", "Black Mountain", "NC", "28711", "Condominium",
        TestData.Purchase.IntendedUseId, TestData.Purchase.IntendedUseVersion, TestData.Purchase.Name,
        "LCU-2026-1203");

    /// <summary>The draft <see cref="Command1"/> starts, by ClientUser1.</summary>
    public static OrderDraftStarted DraftStarted1(string orderId, DateTimeOffset? occurredAt = null)
        => DraftStarted(Command1, orderId, RequestedByUsers.ClientUser1, occurredAt);

    /// <summary>The draft <see cref="Command2"/> starts, by ClientUser2.</summary>
    public static OrderDraftStarted DraftStarted2(string orderId, DateTimeOffset? occurredAt = null)
        => DraftStarted(Command2, orderId, RequestedByUsers.ClientUser2, occurredAt);

    /// <summary>The draft <see cref="Command3"/> starts, by ClientUser1.</summary>
    public static OrderDraftStarted DraftStarted3(string orderId, DateTimeOffset? occurredAt = null)
        => DraftStarted(Command3, orderId, RequestedByUsers.ClientUser1, occurredAt);

    /// <summary>
    ///     The event the order grain records when <paramref name="initiatedBy"/> starts a draft with <paramref name="command"/>;
    ///     use it directly for a one-off variation, such as <c>Command1 with { LoanNumber = ... }</c>.
    /// </summary>
    public static OrderDraftStarted DraftStarted(
        StartOrderDraftCommand command, string orderId, ClaimsPrincipal initiatedBy, DateTimeOffset? occurredAt = null) => new()
    {
        OrderId = orderId,
        ClientId = command.ClientId,
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
        InitiatedBy = initiatedBy.GetTenantUserId(),
        OccurredAt = occurredAt ?? TestData.At
    };
}
