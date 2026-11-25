namespace GetRDone.Orders.Abstractions;

/// <summary>
///     The roles of the AMC's staff, as GetRDone.Identity issues them in the <c>role</c> claim (<c>GetRDoneRoles</c>); keep
///     them in sync. The grains check them to decide who may work on orders, and the web app to decide what to show.
/// </summary>
public static class StaffRoles
{
    public const string Coordinator = "amc-coordinator";
    public const string Reviewer = "amc-reviewer";

    /// <summary>The Client Services Director, such as Scott Riggsbee, who assigns new orders.</summary>
    public const string ClientServicesDirector = "amc-client-services-director";

    /// <summary>The tenant's administrator, who may do anything the Client Services Director may.</summary>
    public const string TenantAdmin = "tenant-admin";

    /// <summary>Every staff role, comma-separated for <c>[Authorize(Roles = ...)]</c>.</summary>
    public const string AnyStaff = Coordinator + "," + Reviewer + "," + ClientServicesDirector + "," + TenantAdmin;

    /// <summary>The roles that may assign orders, comma-separated for <c>[Authorize(Roles = ...)]</c>.</summary>
    public const string AnyOrderAssigner = ClientServicesDirector + "," + TenantAdmin;

    /// <summary>Every staff role, who may see the staff dashboard and the order pages.</summary>
    public static IReadOnlyList<string> Staff { get; } = [Coordinator, Reviewer, ClientServicesDirector, TenantAdmin];

    /// <summary>The roles that may assign orders: the Client Services Director's or higher.</summary>
    public static IReadOnlyList<string> OrderAssigners { get; } = [ClientServicesDirector, TenantAdmin];
}
