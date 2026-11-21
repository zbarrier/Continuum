using System.Security.Claims;

using Continuum.Orleans.Tenancy;

using Duende.IdentityModel;

namespace GetRDone.Orders.Grains.Tests.Data;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Gets the user's IdentityServer subject ID.</summary>
    public static Guid GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        ArgumentNullException.ThrowIfNull(claimsPrincipal);

        var userId = claimsPrincipal.FindFirstValue(JwtClaimTypes.Subject);
        if (string.IsNullOrEmpty(userId))
        {
            throw new InvalidOperationException("UserId (sub) claim is missing.");
        }

        return Guid.Parse(userId);
    }

    /// <summary>Gets the user's tenant from the <c>tenant_id</c> claim.</summary>
    public static string GetTenantId(this ClaimsPrincipal claimsPrincipal)
    {
        ArgumentNullException.ThrowIfNull(claimsPrincipal);

        var tenantId = claimsPrincipal.FindFirstValue(RequestedByUsers.TenantIdClaim);
        if (string.IsNullOrEmpty(tenantId))
        {
            throw new InvalidOperationException("TenantId (tenant_id) claim is missing.");
        }

        return tenantId;
    }

    /// <summary>Gets the tenant-qualified user ID the grains record, as <c>{TenantId}|{UserGuid}</c>.</summary>
    public static string GetTenantUserId(this ClaimsPrincipal claimsPrincipal)
        => TenantKey.Create(claimsPrincipal.GetTenantId(), claimsPrincipal.GetUserId().ToString());
}
