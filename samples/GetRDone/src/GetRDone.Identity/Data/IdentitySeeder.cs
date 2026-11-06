using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GetRDone.Identity.Data;

/// <summary>Applies EF migrations and, in Development, seeds the roles and demo users.</summary>
public static class IdentitySeeder
{
    public const string DemoTenant = "demo";
    public const string DemoPassword = "Pass123$";

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
        await EnsureUserAsync(userManager, "client@getrdone.local", "Casey Client", GetRDoneRoles.Client, DemoTenant);
        await EnsureUserAsync(userManager, "coordinator@getrdone.local", "Morgan Coordinator", GetRDoneRoles.Coordinator, DemoTenant);
        await EnsureUserAsync(userManager, "reviewer@getrdone.local", "Riley Reviewer", GetRDoneRoles.Reviewer, DemoTenant);
        // Vendors are in the global vendor pool, not a tenant.
        await EnsureUserAsync(userManager, "vendor@getrdone.local", "Val Vendor", GetRDoneRoles.Vendor, tenantId: null);
    }

    private static async Task EnsureUserAsync(UserManager<ApplicationUser> userManager, string email, string displayName, string role, string? tenantId)
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
