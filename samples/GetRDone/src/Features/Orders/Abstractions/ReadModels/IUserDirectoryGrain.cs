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
    string? ClientId);
