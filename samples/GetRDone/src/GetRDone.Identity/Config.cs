using System.Security.Claims;

using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;

namespace GetRDone.Identity;

/// <summary>In-memory IdentityServer configuration and seeded demo users for local development.</summary>
public static class Config
{
    public const string TenantClaim = "tenant_id";
    public const string DemoTenant = "demo";
    public const string DemoPassword = "Pass123$";

    public static IEnumerable<IdentityResource> IdentityResources =>
    [
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
        new IdentityResources.Email(),
        new IdentityResource("getrdone", "Get-R-Done profile", [TenantClaim, "role"]),
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

    public static List<TestUser> Users =>
    [
        CreateUser("8d7c5f2a-1b2c-4e5f-9a10-000000000001", "client", "Casey Client", "client"),
        CreateUser("8d7c5f2a-1b2c-4e5f-9a10-000000000002", "coordinator", "Morgan Coordinator", "amc-coordinator"),
        CreateUser("8d7c5f2a-1b2c-4e5f-9a10-000000000003", "reviewer", "Riley Reviewer", "amc-reviewer"),
        CreateUser("8d7c5f2a-1b2c-4e5f-9a10-000000000004", "vendor", "Val Vendor", "vendor"),
    ];

    private static TestUser CreateUser(string subjectId, string username, string name, string role) => new()
    {
        SubjectId = subjectId,
        Username = username,
        Password = DemoPassword,
        Claims =
        [
            new Claim("name", name),
            new Claim("email", $"{username}@getrdone.local"),
            new Claim("role", role),
            new Claim(TenantClaim, DemoTenant),
        ],
    };
}
