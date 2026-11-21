using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.ReadModels;

using Microsoft.AspNetCore.Authorization;

namespace GetRDone.Orders.Grains.ReadModels;

[Authorize]
[GrainType("userDirectory")]
public sealed class UserDirectoryGrain(
    [PersistentState("userDirectory")] IPersistentState<UserDirectoryState> state)
    : Grain, IUserDirectoryGrain
{
    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);
        if (!state.RecordExists && tenantId == SeedData.TenantId)
        {
            state.State.Users = SeedData.Users;
            await state.WriteStateAsync(cancellationToken);
        }
        await base.OnActivateAsync(cancellationToken);
    }

    public Task<UserDirectoryEntry[]> GetUsersAsync() => Task.FromResult(state.State.Users.ToArray());

    public Task<Result> IsClientUserAsync(UserId userId, ClientId clientId)
    {
        var isClientUser = state.State.Users.Any(item =>
            item.UserId == (string)userId
            && item.ClientId == (string)clientId);

        return Task.FromResult(isClientUser
            ? Result.Success()
            : Result.Failure(RequestErrors.NewNotFound("The user is not a user of the client.")));
    }
}

[GenerateSerializer]
public sealed class UserDirectoryState
{
    [Id(0)]
    public UserDirectoryEntry[] Users { get; set; } = [];
}
