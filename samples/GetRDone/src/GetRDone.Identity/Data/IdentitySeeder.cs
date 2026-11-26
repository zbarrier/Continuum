using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GetRDone.Identity.Data;

/// <summary>Applies EF migrations and, in Development, seeds the roles and demo users.</summary>
public static class IdentitySeeder
{
    public const string DemoTenant = "elliott";
    public const string DemoPassword = "Pass123$";

    /// <summary>Ana Ruiz, the Carter Bank &amp; Trust client user from the event model.</summary>
    /// <remarks>The Orders read models seed the same user (GetRDone.Orders.Grains.ReadModels.SampleSeedData); keep them in sync.</remarks>
    public const string AnaRuizUserId = "7304a1f2-3c5d-4e6f-8a9b-0c1d2e3f4a5b";

    /// <summary>Scott Riggsbee, Elliott &amp; Company's Client Services Director, who is told about new orders.</summary>
    /// <remarks>The Orders read models seed the same user (GetRDone.Orders.Grains.ReadModels.SampleSeedData); keep them in sync.</remarks>
    public const string ScottRiggsbeeUserId = "3005c2d8-6e1f-4a7b-9c3d-5e8f1a2b4c6d";

    /// <summary>Morgan Coordinator, an AMC coordinator, who can work on orders but not assign them.</summary>
    /// <remarks>The Orders read models seed the same user (GetRDone.Orders.Grains.ReadModels.SampleSeedData); keep them in sync.</remarks>
    public const string MorganCoordinatorUserId = "5b2e7c41-8d3f-4a6b-9c1e-2f4a6b8d0c13";

    /// <summary>Riley Reviewer, an AMC reviewer.</summary>
    /// <remarks>The Orders read models seed the same user (GetRDone.Orders.Grains.ReadModels.SampleSeedData); keep them in sync.</remarks>
    public const string RileyReviewerUserId = "6c3f8d52-9e4a-4b7c-8d2f-3a5b7c9e1d24";

    /// <summary>Avery Admin, the tenant admin, who can do anything the Client Services Director can.</summary>
    /// <remarks>The Orders read models seed the same user (GetRDone.Orders.Grains.ReadModels.SampleSeedData); keep them in sync.</remarks>
    public const string AveryAdminUserId = "7d4a9e63-0f5b-4c8d-9e3a-4b6c8d0f2e35";

    /// <summary>The service account the Order Number Assigner automation runs as.</summary>
    /// <remarks>The worker signs the automation in with the same ID (GetRDone.Orders.Abstractions.AutomationUser); keep them in sync.</remarks>
    public const string OrderNumberAssignerUserId = "0a1b2c01-5d3e-4f7a-8b9c-1d2e3f4a5b6c";

    /// <summary>The service account the New Order Notifier automation runs as.</summary>
    /// <remarks>The worker signs the automation in with the same ID (GetRDone.Orders.Abstractions.AutomationUser); keep them in sync.</remarks>
    public const string NewOrderNotifierUserId = "0a1b2c02-6e4f-4a8b-9c0d-2e3f4a5b6c7d";

    /// <summary>The service account the Document Request Notifier automation runs as.</summary>
    /// <remarks>The silo signs the automation in with the same ID (GetRDone.Orders.Abstractions.AutomationUser); keep them in sync.</remarks>
    public const string DocumentRequestNotifierUserId = "0a1b2c03-7f5a-4b9c-8d1e-3f4a5b6c7d8e";

    /// <summary>The service account the Description of Work Approval Notifier automation runs as.</summary>
    /// <remarks>The silo signs the automation in with the same ID (GetRDone.Orders.Abstractions.AutomationUser); keep them in sync.</remarks>
    public const string DescriptionOfWorkApprovalNotifierUserId = "0a1b2c04-8a6b-4c0d-9e2f-4a5b6c7d8e9f";

    /// <summary>The service account the Description of Work Approved Notifier automation runs as.</summary>
    /// <remarks>The silo signs the automation in with the same ID (GetRDone.Orders.Abstractions.AutomationUser); keep them in sync.</remarks>
    public const string DescriptionOfWorkApprovedNotifierUserId = "0a1b2c05-9b7c-4d1e-8f3a-5b6c7d8e9f0a";

    /// <summary>The service account the Vendor Assignment Notifier automation runs as.</summary>
    /// <remarks>The silo signs the automation in with the same ID (GetRDone.Orders.Abstractions.AutomationUser); keep them in sync.</remarks>
    public const string VendorAssignmentNotifierUserId = "0a1b2c06-ac8d-4e2f-9a4b-6c7d8e9f0a1b";

    /// <summary>Mike Miller of Miller Valuation Group, the vendor Elliott &amp; Company assigns orders to in the event model.</summary>
    /// <remarks>The Orders read models seed the same user (GetRDone.Orders.Grains.ReadModels.SampleSeedData); keep them in sync.</remarks>
    public const string MikeMillerUserId = "5701c3e9-2a4b-4c6d-8e0f-1a2b3c4d5e6f";

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
        await EnsureUserAsync(userManager, "scott.riggsbee@elliott.example", "Scott Riggsbee", GetRDoneRoles.ClientServicesDirector, DemoTenant, ScottRiggsbeeUserId);
        await EnsureUserAsync(userManager, "coordinator@getrdone.local", "Morgan Coordinator", GetRDoneRoles.Coordinator, DemoTenant, MorganCoordinatorUserId);
        await EnsureUserAsync(userManager, "reviewer@getrdone.local", "Riley Reviewer", GetRDoneRoles.Reviewer, DemoTenant, RileyReviewerUserId);
        await EnsureUserAsync(userManager, "admin@getrdone.local", "Avery Admin", GetRDoneRoles.TenantAdmin, DemoTenant, AveryAdminUserId);
        // Mike works for a vendor on Elliott & Company's own list, so he belongs to the tenant.
        await EnsureUserAsync(userManager, "mike@millervaluation.example", "Mike Miller", GetRDoneRoles.Vendor, DemoTenant, MikeMillerUserId);
        // Self-registered vendors are in the global vendor pool, not a tenant.
        await EnsureUserAsync(userManager, "vendor@getrdone.local", "Val Vendor", GetRDoneRoles.Vendor, tenantId: null);

        // The automations' service accounts have no password, so nobody can sign in as them; the silo signs them in itself.
        await EnsureUserAsync(userManager, "order-number-assigner@elliott.example", "Order Number Assigner", GetRDoneRoles.Automation,
            DemoTenant, OrderNumberAssignerUserId, password: null);
        await EnsureUserAsync(userManager, "new-order-notifier@elliott.example", "New Order Notifier", GetRDoneRoles.Automation,
            DemoTenant, NewOrderNotifierUserId, password: null);
        await EnsureUserAsync(userManager, "document-request-notifier@elliott.example", "Document Request Notifier", GetRDoneRoles.Automation,
            DemoTenant, DocumentRequestNotifierUserId, password: null);
        await EnsureUserAsync(userManager, "dow-approval-notifier@elliott.example", "Description of Work Approval Notifier",
            GetRDoneRoles.Automation, DemoTenant, DescriptionOfWorkApprovalNotifierUserId, password: null);
        await EnsureUserAsync(userManager, "dow-approved-notifier@elliott.example", "Description of Work Approved Notifier",
            GetRDoneRoles.Automation, DemoTenant, DescriptionOfWorkApprovedNotifierUserId, password: null);
        await EnsureUserAsync(userManager, "vendor-assignment-notifier@elliott.example", "Vendor Assignment Notifier",
            GetRDoneRoles.Automation, DemoTenant, VendorAssignmentNotifierUserId, password: null);
    }

    /// <summary>
    ///     Creates the user with exactly <paramref name="role"/>, or gives an existing user that role in place of their others,
    ///     so a demo user whose role changed gets the new one without the database being reset.
    /// </summary>
    private static async Task EnsureUserAsync(UserManager<ApplicationUser> userManager, string email, string displayName, string role,
        string? tenantId, string? userId = null, string? password = DemoPassword)
    {
        if (await userManager.FindByEmailAsync(email) is { } existing)
        {
            var roles = await userManager.GetRolesAsync(existing);
            var otherRoles = roles.Where(item => item != role).ToArray();
            if (otherRoles.Length > 0)
            {
                EnsureSucceeded(await userManager.RemoveFromRolesAsync(existing, otherRoles));
            }
            if (!roles.Contains(role))
            {
                EnsureSucceeded(await userManager.AddToRoleAsync(existing, role));
            }
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

        EnsureSucceeded(password is null
            ? await userManager.CreateAsync(user)
            : await userManager.CreateAsync(user, password));
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
