namespace GetRDone.Identity.Data;

public static class GetRDoneRoles
{
    public const string Client = "client";
    public const string Coordinator = "amc-coordinator";
    public const string Reviewer = "amc-reviewer";
    public const string Vendor = "vendor";

    public static IReadOnlyList<string> All { get; } = [Client, Coordinator, Reviewer, Vendor];
}
