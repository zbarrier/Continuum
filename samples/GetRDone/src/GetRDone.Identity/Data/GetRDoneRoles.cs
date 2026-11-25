namespace GetRDone.Identity.Data;

public static class GetRDoneRoles
{
    public const string Client = "client";
    public const string Coordinator = "amc-coordinator";
    public const string Reviewer = "amc-reviewer";

    /// <summary>The Client Services Director, who assigns new orders.</summary>
    public const string ClientServicesDirector = "amc-client-services-director";

    /// <summary>The tenant's administrator, who may do anything the Client Services Director may.</summary>
    public const string TenantAdmin = "tenant-admin";

    public const string Vendor = "vendor";

    /// <summary>The service accounts in-silo automations run as; nobody signs in with them.</summary>
    public const string Automation = "automation";

    /// <remarks>The Orders grains check the staff roles by name (GetRDone.Orders.Abstractions.StaffRoles); keep them in sync.</remarks>
    public static IReadOnlyList<string> All { get; } = [Client, Coordinator, Reviewer, ClientServicesDirector, TenantAdmin, Vendor, Automation];
}
