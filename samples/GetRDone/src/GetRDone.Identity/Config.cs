using Duende.IdentityServer;
using Duende.IdentityServer.Models;

using GetRDone.Identity.Data;

namespace GetRDone.Identity;

/// <summary>In-memory IdentityServer resources and clients. Users live in ASP.NET Core Identity (SQL Server).</summary>
public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
    [
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
        new IdentityResources.Email(),
        new IdentityResource("getrdone", "Get-R-Done profile", [GetRDoneUserClaimsPrincipalFactory.TenantClaim, "role"]),
    ];

    public static IEnumerable<Client> Clients(string webBaseUrl) =>
    [
        new Client
        {
            ClientId = "getrdone-web",
            ClientName = "Get-R-Done Web",
            ClientSecrets = { new Secret("getrdone-web-secret".Sha256()) },
            AllowedGrantTypes = GrantTypes.Code,
            RedirectUris = { $"{webBaseUrl}/signin-oidc" },
            PostLogoutRedirectUris = { $"{webBaseUrl}/signout-callback-oidc" },
            AllowedScopes =
            {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
                IdentityServerConstants.StandardScopes.Email,
                "getrdone",
            },
            AlwaysIncludeUserClaimsInIdToken = true,
        },
    ];
}
