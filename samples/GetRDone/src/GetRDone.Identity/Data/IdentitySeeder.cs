using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GetRDone.Identity.Data;

/// <summary>Applies EF migrations and, in Development, seeds the roles and demo users.</summary>
public static class IdentitySeeder
{
    public const string DemoTenant = "elliott";
    public const string DemoPassword = "Pass123$";

    /// <summary>Ana Ruiz, the Carter Bank &amp; Trust client user from the event model.</summary>
    /// <remarks>The Orders read models seed the same user (GetRDone.Orders.Grains.ReadModels.SeedData); keep them in sync.</remarks>
    public const string AnaRuizUserId = "7304a1f2-3c5d-4e6f-8a9b-0c1d2e3f4a5b";

    public static async Task MigrateAndSeedAsync(IServiceProvider services, bool seedDemoUsers)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in GetRDoneRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                EnsureSucceeded(await roleManager.CreateAsync(new IdentityRole(role)));
            }
        }

        if (!seedDemoUsers)
        {
            return;
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        await EnsureUserAsync(userManager, "ana.ruiz@carterbank.example", "Ana Ruiz", GetRDoneRoles.Client, DemoTenant, AnaRuizUserId);
        await EnsureUserAsync(userManager, "coordinator@getrdone.local", "Morgan Coordinator", GetRDoneRoles.Coordinator, DemoTenant);
        await EnsureUserAsync(userManager, "reviewer@getrdone.local", "Riley Reviewer", GetRDoneRoles.Reviewer, DemoTenant);
        // Vendors are in the global vendor pool, not a tenant.
        await EnsureUserAsync(userManager, "vendor@getrdone.local", "Val Vendor", GetRDoneRoles.Vendor, tenantId: null);
    }

    private static async Task EnsureUserAsync(UserManager<ApplicationUser> userManager, string email, string displayName, string role, string? tenantId, string? userId = null)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = displayName,
            TenantId = tenantId,
        };
        if (userId is not null)
        {
            user.Id = userId;
        }

        EnsureSucceeded(await userManager.CreateAsync(user, DemoPassword));
        EnsureSucceeded(await userManager.AddToRoleAsync(user, role));
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }
    }
}
