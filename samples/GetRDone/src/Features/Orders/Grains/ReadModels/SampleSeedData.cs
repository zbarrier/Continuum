using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.ReadModels;

namespace GetRDone.Orders.Grains.ReadModels;

/// <summary>
///     The reference data the sample seeds into its read models, matching the event model's seed data chapter. A full
///     application would project these read models from the events of the aggregates that own the data.
/// </summary>
public sealed class SampleSeedData : IReadModelSeedData
{
    public const string TenantId = "elliott";

    public const string CarterBankAndTrustClientId = "elliott|c8e41a7d";

    /// <summary>Ana Ruiz, a client user of Carter Bank &amp; Trust.</summary>
    /// <remarks>GetRDone.Identity seeds the same user ID (IdentitySeeder.AnaRuizUserId); keep them in sync.</remarks>
    public const string AnaRuizUserId = "elliott|7304a1f2-3c5d-4e6f-8a9b-0c1d2e3f4a5b";

    /// <summary>Scott Riggsbee, Elliott &amp; Company's Client Services Director, who is told about new orders.</summary>
    /// <remarks>GetRDone.Identity seeds the same user ID (IdentitySeeder.ScottRiggsbeeUserId); keep them in sync.</remarks>
    public const string ScottRiggsbeeUserId = "elliott|3005c2d8-6e1f-4a7b-9c3d-5e8f1a2b4c6d";

    /// <summary>Morgan Coordinator, an AMC coordinator, who can work on orders but not assign them.</summary>
    /// <remarks>GetRDone.Identity seeds the same user ID (IdentitySeeder.MorganCoordinatorUserId); keep them in sync.</remarks>
    public const string MorganCoordinatorUserId = "elliott|5b2e7c41-8d3f-4a6b-9c1e-2f4a6b8d0c13";

    /// <summary>Riley Reviewer, an AMC reviewer.</summary>
    /// <remarks>GetRDone.Identity seeds the same user ID (IdentitySeeder.RileyReviewerUserId); keep them in sync.</remarks>
    public const string RileyReviewerUserId = "elliott|6c3f8d52-9e4a-4b7c-8d2f-3a5b7c9e1d24";

    /// <summary>Avery Admin, Elliott &amp; Company's tenant admin.</summary>
    /// <remarks>GetRDone.Identity seeds the same user ID (IdentitySeeder.AveryAdminUserId); keep them in sync.</remarks>
    public const string AveryAdminUserId = "elliott|7d4a9e63-0f5b-4c8d-9e3a-4b6c8d0f2e35";

    /// <summary>Miller Valuation Group, the vendor Elliott &amp; Company assigns orders to in the event model.</summary>
    public const string MillerValuationGroupVendorId = "elliott|61a7d2c9";

    /// <summary>Mike Miller, the owner of Miller Valuation Group.</summary>
    /// <remarks>GetRDone.Identity seeds the same user ID (IdentitySeeder.MikeMillerUserId); keep them in sync.</remarks>
    public const string MikeMillerUserId = "elliott|5701c3e9-2a4b-4c6d-8e0f-1a2b3c4d5e6f";

    private const string ElliottAndCompany = "Elliott & Company";
    private const string MillerValuationGroup = "Miller Valuation Group";

    private static readonly ReadModelSeed Seed = new(
        IntendedUses:
        [
            new("elliott|ad86c5b7", 1, "Refinance"),
            new("elliott|b3c94e12", 1, "Purchase"),
            new("elliott|4c19e7a2", 1, "Construction"),
            new("elliott|e81d3f06", 1, "Other")
        ],
        DocumentTypes:
        [
            new("elliott|3e7a9c15", 1, "Lease agreements"),
            new("elliott|5f1c8e24", 1, "Rent roll"),
            new("elliott|9b4d2a71", 1, "Operating statements"),
            new("elliott|7d2e9b40", 1, "Other")
        ],
        OrderableServices:
        [
            new("elliott|5a1f0c3e", 1, "Appraisal Commercial",
            [
                new("elliott|e5a1c9d3", 1, "Market Value Interior Inspection"),
                new("elliott|f3a8d215", 1, "Market Value Exterior Inspection")
            ]),
            new("elliott|7d2e4b90", 1, "Appraisal Residential",
            [
                new("elliott|1b6c9e47", 1, "Uniform Residential Appraisal Report (1004)"),
                new("elliott|9e4f2a68", 1, "Exterior-Only Inspection (2055)")
            ])
        ],
        Users:
        [
            new(AnaRuizUserId, "Ana Ruiz", "Commercial Loan Officer", "Carter Bank & Trust", "ana.ruiz@carterbank.example",
                CarterBankAndTrustClientId),
            new(ScottRiggsbeeUserId, "Scott Riggsbee", UserDirectoryEntry.ClientServicesDirectorTitle, ElliottAndCompany,
                "scott.riggsbee@elliott.example", null),
            new(MorganCoordinatorUserId, "Morgan Coordinator", "Coordinator", ElliottAndCompany, "coordinator@getrdone.local", null),
            new(RileyReviewerUserId, "Riley Reviewer", "Reviewer", ElliottAndCompany, "reviewer@getrdone.local", null),
            new(AveryAdminUserId, "Avery Admin", "Tenant Admin", ElliottAndCompany, "admin@getrdone.local", null),
            new(MikeMillerUserId, "Mike Miller", "Owner, Certified General Appraiser", MillerValuationGroup,
                "mike@millervaluation.example", null, MillerValuationGroupVendorId),
            Automation(AutomationUser.OrderNumberAssigner, "order-number-assigner@elliott.example"),
            Automation(AutomationUser.NewOrderNotifier, "new-order-notifier@elliott.example"),
            Automation(AutomationUser.DocumentRequestNotifier, "document-request-notifier@elliott.example"),
            Automation(AutomationUser.DescriptionOfWorkApprovalNotifier, "dow-approval-notifier@elliott.example"),
            Automation(AutomationUser.DescriptionOfWorkApprovedNotifier, "dow-approved-notifier@elliott.example"),
            Automation(AutomationUser.VendorAssignmentNotifier, "vendor-assignment-notifier@elliott.example"),
            Automation(AutomationUser.ReportDeliveredNotifier, "report-delivered-notifier@elliott.example")
        ],
        Vendors:
        [
            new(MillerValuationGroupVendorId, MillerValuationGroup, ["VA"])
        ]);

    public ReadModelSeed? GetSeed(string? tenantId) => tenantId == TenantId ? Seed : null;

    private static UserDirectoryEntry Automation(AutomationUser account, string email)
        => new(account.GetTenantUserId(TenantId), account.DisplayName, UserDirectoryEntry.AutomationTitle, ElliottAndCompany, email, null);
}
