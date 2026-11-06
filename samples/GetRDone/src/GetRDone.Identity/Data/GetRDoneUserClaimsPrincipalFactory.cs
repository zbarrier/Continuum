using System.Security.Claims;

using Duende.IdentityModel;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace GetRDone.Identity.Data;

/// <summary>Adds the Get-R-Done claims (display name and tenant) to signed-in users.</summary>
public sealed class GetRDoneUserClaimsPrincipalFactory(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>(userManager, roleManager, options)
{
    public const string TenantClaim = "tenant_id";

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        if (!string.IsNullOrWhiteSpace(user.DisplayName))
        {
            foreach (var claim in identity.FindAll(JwtClaimTypes.Name).ToList())
            {
                identity.RemoveClaim(claim);
            }

            identity.AddClaim(new Claim(JwtClaimTypes.Name, user.DisplayName));
        }

        if (!string.IsNullOrWhiteSpace(user.TenantId))
        {
            identity.AddClaim(new Claim(TenantClaim, user.TenantId));
        }

        return identity;
    }
}
