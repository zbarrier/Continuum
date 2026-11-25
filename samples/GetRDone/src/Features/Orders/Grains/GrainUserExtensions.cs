using System.Security.Claims;

using Continuum.Orleans.Tenancy;

using ManagedCode.Orleans.Identity.Core.Extensions;

namespace GetRDone.Orders.Grains;

internal static class GrainUserExtensions
{
    /// <summary>
    ///     Gets the current user's ID qualified by <paramref name="tenantId"/>, as <c>{TenantId}|{UserGuid}</c>. The identity
    ///     provider only knows the user GUID; a full application would also check the user's access to the tenant.
    /// </summary>
    public static string? GetCurrentTenantUserId(this Grain grain, string? tenantId)
    {
        var user = grain.GetCurrentUser();
        var userGuid = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userGuid))
        {
            return null;
        }
        return tenantId is null ? userGuid : $"{tenantId}{TenantKey.Separator}{userGuid}";
    }

    /// <summary>
    ///     Whether the user has any of the roles, from the <c>role</c> claims GetRDone.Identity issues or the
    ///     <see cref="ClaimTypes.Role"/> claims of a principal mapped to the .NET claim types.
    /// </summary>
    public static bool IsInAnyRole(this ClaimsPrincipal user, IReadOnlyList<string> roles)
        => user.Claims.Any(claim =>
            claim.Type is "role" or ClaimTypes.Role && roles.Contains(claim.Value, StringComparer.Ordinal));
}
