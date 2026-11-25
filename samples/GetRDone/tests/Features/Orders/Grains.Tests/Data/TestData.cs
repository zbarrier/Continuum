using System.Security.Claims;

using Duende.IdentityModel;

using GetRDone.Orders.Abstractions.ReadModels;

namespace GetRDone.Orders.Grains.Tests.Data;

/// <summary>The reference data the tests seed into the read models, independent of the sample's seed data.</summary>
public static class TestData
{
    /// <summary>Summit Valuation, the AMC tenant the tests run in.</summary>
    public const string TenantId = "summit";

    /// <summary>A tenant with no seeded data.</summary>
    public const string OtherTenantId = "other";

    /// <summary>The time of events in a Given, when the test doesn't care when they occurred.</summary>
    public static DateTimeOffset At { get; } = new(2026, 11, 17, 9, 5, 0, TimeSpan.Zero);

    public const string LakeviewCreditUnionClientId = "summit|9b3e7d21";
    public const string HarborNationalBankClientId = "summit|2f6a8c14";

    public static IntendedUse Refinance { get; } = new("summit|7a1d0c42", 2, "Refinance");
    public static IntendedUse Purchase { get; } = new("summit|3e8b5f19", 1, "Purchase");

    public static DocumentType PurchaseContract { get; } = new("summit|4a8e2c61", 2, "Purchase contract");
    public static DocumentType Survey { get; } = new("summit|c3f7b905", 1, "Survey");

    public static OrderableFormType UniformResidentialAppraisal { get; } = new("summit|d05e9b37", 1, "Uniform Residential Appraisal Report");
    public static OrderableFormType DesktopAppraisal { get; } = new("summit|8e2b6f41", 2, "Desktop Appraisal");

    public static OrderableService ResidentialAppraisal { get; } = new(
        "summit|6c4f2a90", 3, "Residential Appraisal", [UniformResidentialAppraisal, DesktopAppraisal]);

    public static OrderableFormType NarrativeAppraisal { get; } = new("summit|a2c6f8e0", 1, "Narrative Appraisal Report");
    public static OrderableFormType RestrictedAppraisal { get; } = new("summit|5d9b3c72", 2, "Restricted Appraisal Report");

    public static OrderableService CommercialAppraisal { get; } = new(
        "summit|4b7e1d03", 1, "Commercial Appraisal", [NarrativeAppraisal, RestrictedAppraisal]);

    public static UserDirectoryEntry ClientUser1 { get; } = DirectoryEntry(
        RequestedByUsers.ClientUser1, "Loan Officer", "Lakeview Credit Union", LakeviewCreditUnionClientId);

    public static UserDirectoryEntry ClientUser2 { get; } = DirectoryEntry(
        RequestedByUsers.ClientUser2, "Loan Processor", "Harbor National Bank", HarborNationalBankClientId);

    /// <summary>Tommy Jones, the test tenant's Client Services Director, who is told about new orders.</summary>
    public static UserDirectoryEntry Coordinator1 { get; } = DirectoryEntry(
        RequestedByUsers.Coordinator1, UserDirectoryEntry.ClientServicesDirectorTitle, "Summit Valuation", clientId: null);

    public static IntendedUse[] IntendedUses { get; } = [Refinance, Purchase];

    public static DocumentType[] DocumentTypes { get; } = [PurchaseContract, Survey];

    public static OrderableService[] OrderableServices { get; } = [ResidentialAppraisal, CommercialAppraisal];

    public static UserDirectoryEntry[] Users { get; } = [ClientUser1, ClientUser2, Coordinator1];

    private static UserDirectoryEntry DirectoryEntry(ClaimsPrincipal user, string title, string organizationName, string? clientId)
        => new(user.GetTenantUserId(), user.Identity!.Name!, title, organizationName, user.FindFirstValue(JwtClaimTypes.Email)!, clientId);
}
