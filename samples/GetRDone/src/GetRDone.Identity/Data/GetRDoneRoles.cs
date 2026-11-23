namespace GetRDone.Identity.Data;

public static class GetRDoneRoles
{
    public const string Client = "client";
    public const string Coordinator = "amc-coordinator";
    public const string Reviewer = "amc-reviewer";
    public const string Vendor = "vendor";

    /// <summary>The service accounts in-silo automations run as; nobody signs in with them.</summary>
    public const string Automation = "automation";

    public static IReadOnlyList<string> All { get; } = [Client, Coordinator, Reviewer, Vendor, Automation];
}
