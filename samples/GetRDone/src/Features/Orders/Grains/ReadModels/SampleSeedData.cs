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

    private static readonly ReadModelSeed Seed = new(
        IntendedUses:
        [
            new("elliott|ad86c5b7", 1, "Refinance"),
            new("elliott|b3c94e12", 1, "Purchase"),
            new("elliott|4c19e7a2", 1, "Construction"),
            new("elliott|e81d3f06", 1, "Other")
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
                CarterBankAndTrustClientId)
        ]);

    public ReadModelSeed? GetSeed(string? tenantId) => tenantId == TenantId ? Seed : null;
}
