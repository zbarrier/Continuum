using Microsoft.AspNetCore.Identity;

namespace GetRDone.Identity.Data;

public sealed class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>The tenant the user belongs to; null for users outside a single tenant, such as self-registered vendors.</summary>
    public string? TenantId { get; set; }
}
