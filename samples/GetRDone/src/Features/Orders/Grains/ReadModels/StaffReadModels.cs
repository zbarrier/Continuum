using System.Net;

using Continuum.CSharpFunctionalExtensions;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.ReadModels;

using ManagedCode.Orleans.Identity.Core.Extensions;

namespace GetRDone.Orders.Grains.ReadModels;

/// <summary>Helpers for the read models only the AMC's staff can read: the staff dashboard and the staff order page.</summary>
/// <remarks>
///     The projections store user and client IDs and turn them into names when they are read, from the tenant's user
///     directory. Stream deliveries carry no signed-in user, so a projection can't call the directory while it applies an
///     event; a query carries the staff member who asked, and the names are always current.
/// </remarks>
internal static class StaffReadModels
{
    /// <summary>Fails with permission denied unless the current user is one of the AMC's staff.</summary>
    public static Result EnsureStaff(this Grain grain)
        => grain.GetCurrentUser().IsInAnyRole(StaffRoles.Staff)
            ? Result.Success()
            : Result.Failure(RequestErrors.New(HttpStatusCode.Forbidden, Grpc.Core.StatusCode.PermissionDenied,
                "Only the AMC's staff can see orders they did not start."));

    /// <summary>Fails with invalid argument unless the page is valid.</summary>
    public static Result EnsureValidPage(int skip, int take)
        => skip >= 0 && take >= 1
            ? Result.Success()
            : Result.Failure(RequestErrors.NewInvalidArg("Skip must not be negative and take must be at least one."));
}

/// <summary>The names of a tenant's users, clients and vendors, from its user directory and its assignable vendors.</summary>
internal sealed class UserDirectoryNames
{
    private readonly Dictionary<string, UserDirectoryEntry> _users;
    private readonly Dictionary<string, string> _clientNames;
    private readonly Dictionary<string, string> _vendorNames;

    private UserDirectoryNames(UserDirectoryEntry[] users, AssignableVendor[] vendors)
    {
        _users = users.ToDictionary(user => user.UserId, StringComparer.Ordinal);
        _clientNames = users
            .Where(user => user.ClientId is not null)
            .DistinctBy(user => user.ClientId)
            .ToDictionary(user => user.ClientId!, user => user.OrganizationName, StringComparer.Ordinal);
        _vendorNames = vendors.ToDictionary(vendor => vendor.VendorId, vendor => vendor.Name, StringComparer.Ordinal);
    }

    public static async Task<UserDirectoryNames> LoadAsync(IGrainFactory grainFactory, string? tenantId)
    {
        var users = grainFactory.GetUserDirectoryGrain(tenantId).GetUsersAsync();
        var vendors = grainFactory.GetAssignableVendorsGrain(tenantId).GetVendorsAsync();
        return new(await users, await vendors);
    }

    /// <summary>The user's display name, or null when the directory doesn't know them.</summary>
    public string? UserName(string? userId) => userId is not null && _users.TryGetValue(userId, out var user) ? user.DisplayName : null;

    /// <summary>The name of the organization the user works for, or null when the directory doesn't know them.</summary>
    public string? OrganizationName(string? userId)
        => userId is not null && _users.TryGetValue(userId, out var user) ? user.OrganizationName : null;

    /// <summary>The vendor the user works for, as a tenant-qualified vendor ID, or null when they are not a vendor user.</summary>
    public string? VendorIdOf(string? userId) => userId is not null && _users.TryGetValue(userId, out var user) ? user.VendorId : null;

    /// <summary>The client's organization name, from any of its users, or null when the directory doesn't know the client.</summary>
    public string? ClientName(string clientId) => _clientNames.GetValueOrDefault(clientId);

    /// <summary>The vendor's name, or null when the tenant's assignable vendors don't list it.</summary>
    public string? VendorName(string? vendorId) => vendorId is not null ? _vendorNames.GetValueOrDefault(vendorId) : null;
}
