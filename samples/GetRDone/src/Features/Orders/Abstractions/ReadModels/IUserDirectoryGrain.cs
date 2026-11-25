using Continuum.CSharpFunctionalExtensions;

using Orleans.Concurrency;

namespace GetRDone.Orders.Abstractions.ReadModels;

/// <summary>
///     The users of a tenant and the client each client user belongs to. The sample seeds them per tenant instead of
///     projecting them from user events.
/// </summary>
public interface IUserDirectoryGrain : IGrainWithStringKey
{
    [ReadOnly]
    Task<UserDirectoryEntry[]> GetUsersAsync();

    /// <summary>Succeeds when the user is a client user of the client.</summary>
    [ReadOnly]
    Task<Result> IsClientUserAsync(UserId userId, ClientId clientId);
}

[GenerateSerializer, Immutable]
public sealed record UserDirectoryEntry(
    string UserId,
    string DisplayName,
    string Title,
    string OrganizationName,
    string Email,
    string? ClientId)
{
    /// <summary>
    ///     The title of the AMC staff member who assigns new orders, and so is told about them. A full application would
    ///     find them by their permissions instead of their title.
    /// </summary>
    public const string ClientServicesDirectorTitle = "Client Services Director";
}
