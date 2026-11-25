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

/// <summary>The names of a tenant's users and clients, from its user directory.</summary>
internal sealed class UserDirectoryNames
{
    private readonly Dictionary<string, string> _userNames;
    private readonly Dictionary<string, string> _clientNames;

    private UserDirectoryNames(UserDirectoryEntry[] users)
    {
        _userNames = users.ToDictionary(user => user.UserId, user => user.DisplayName, StringComparer.Ordinal);
        _clientNames = users
            .Where(user => user.ClientId is not null)
            .DistinctBy(user => user.ClientId)
            .ToDictionary(user => user.ClientId!, user => user.OrganizationName, StringComparer.Ordinal);
    }

    public static async Task<UserDirectoryNames> LoadAsync(IGrainFactory grainFactory, string? tenantId)
        => new(await grainFactory.GetUserDirectoryGrain(tenantId).GetUsersAsync());

    /// <summary>The user's display name, or null when the directory doesn't know them.</summary>
    public string? UserName(string? userId) => userId is not null && _userNames.TryGetValue(userId, out var name) ? name : null;

    /// <summary>The client's organization name, from any of its users, or null when the directory doesn't know the client.</summary>
    public string? ClientName(string clientId) => _clientNames.GetValueOrDefault(clientId);
}
