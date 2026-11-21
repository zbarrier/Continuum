using GetRDone.Orders.Abstractions.ReadModels;

namespace GetRDone.Orders.Grains.ReadModels;

/// <summary>
///     The reference data the sample seeds into its read models, matching the event model's seed data chapter. A full
///     application would project these read models from the events of the aggregates that own the data.
/// </summary>
public static class SeedData
{
    public const string TenantId = "elliott";

    public const string CarterBankAndTrustClientId = "elliott|c8e41a7d";

    /// <summary>Ana Ruiz, a client user of Carter Bank &amp; Trust.</summary>
    /// <remarks>GetRDone.Identity seeds the same user ID (IdentitySeeder.AnaRuizUserId); keep them in sync.</remarks>
    public const string AnaRuizUserId = "elliott|7304a1f2-3c5d-4e6f-8a9b-0c1d2e3f4a5b";

    public static IntendedUse[] IntendedUses { get; } =
    [
        new("elliott|ad86c5b7", 1, "Refinance"),
        new("elliott|b3c94e12", 1, "Purchase"),
        new("elliott|4c19e7a2", 1, "Construction"),
        new("elliott|e81d3f06", 1, "Other")
    ];

    public static OrderableService[] OrderableServices { get; } =
    [
        new("elliott|5a1f0c3e", 1, "Appraisal Commercial", "elliott|e5a1c9d3", 1, "Market Value Interior Inspection")
    ];

    public static UserDirectoryEntry[] Users { get; } =
    [
        new(AnaRuizUserId, "Ana Ruiz", "Commercial Loan Officer", "Carter Bank & Trust", "ana.ruiz@carterbank.example",
            CarterBankAndTrustClientId)
    ];
}
