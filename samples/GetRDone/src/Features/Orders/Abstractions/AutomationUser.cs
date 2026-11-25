using System.Security.Claims;

using Continuum.Orleans.Tenancy;

namespace GetRDone.Orders.Abstractions;

/// <summary>
///     A service account an automation runs as, so its grain calls pass <c>[Authorize]</c> and the events they record
///     name it as <c>InitiatedBy</c>. The event model gives each automation its own account.
/// </summary>
/// <remarks>
///     GetRDone.Identity seeds the same accounts (<c>IdentitySeeder</c>) with no password, so nobody can sign in as them,
///     and <c>SampleSeedData</c> lists them in the user directory; keep the IDs in sync. The worker is a trusted cluster
///     client and a grain's reaction runs in the silo, so an automation signs in by building its own principal, with the
///     claims GetRDone.Identity issues, rather than requesting a token (see <c>SignIn</c> in GetRDone.Orders.Automations
///     and the order's reaction to an issued order number). The grains use the accounts to authorize the automations'
///     commands.
/// </remarks>
public sealed class AutomationUser
{
    /// <summary>The role GetRDone.Identity gives the automation accounts (<c>GetRDoneRoles.Automation</c>).</summary>
    public const string Role = "automation";

    private AutomationUser(string userGuid, string displayName)
    {
        UserGuid = userGuid;
        DisplayName = displayName;
    }

    /// <summary>Gives submitted orders their order numbers.</summary>
    public static AutomationUser OrderNumberAssigner { get; } = new("0a1b2c01-5d3e-4f7a-8b9c-1d2e3f4a5b6c", "Order Number Assigner");

    /// <summary>Tells client services about new orders.</summary>
    public static AutomationUser NewOrderNotifier { get; } = new("0a1b2c02-6e4f-4a8b-9c0d-2e3f4a5b6c7d", "New Order Notifier");

    /// <summary>Emails clients the documents the AMC requests from them.</summary>
    public static AutomationUser DocumentRequestNotifier { get; } = new("0a1b2c03-7f5a-4b9c-8d1e-3f4a5b6c7d8e", "Document Request Notifier");

    /// <summary>The account's user ID in the identity provider.</summary>
    public string UserGuid { get; }

    public string DisplayName { get; }

    /// <summary>Gets the account's ID in <paramref name="tenantId"/>, as events record it: <c>{TenantId}|{UserGuid}</c>.</summary>
    public string GetTenantUserId(string tenantId) => TenantKey.Create(tenantId, UserGuid);

    /// <summary>Creates the principal the account is signed in with when it works on <paramref name="tenantId"/>'s data.</summary>
    public ClaimsPrincipal CreatePrincipal(string tenantId)
    {
        ArgumentException.ThrowIfNullOrEmpty(tenantId);
        return new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("sub", UserGuid),
            new Claim("name", DisplayName),
            new Claim("role", Role),
            new Claim("tenant_id", tenantId)
        ], "GetRDone.Automation", "name", "role"));
    }
}
