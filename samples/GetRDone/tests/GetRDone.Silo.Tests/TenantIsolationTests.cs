using System.Security.Claims;

using Continuum.Orleans.Tenancy;

using ManagedCode.Orleans.Identity.Core.Constants;

namespace GetRDone.Silo.Tests;

/// <summary>
///     Tests that <see cref="TenantIsolationSiloBuilderExtensions.AddTenantIsolation"/> keeps calls inside a tenant:
///     <see cref="ClientCallTenantFilter"/> for calls from cluster clients, and Orleans.Multitenant's communication
///     separation for calls from grains.
/// </summary>
public sealed class TenantIsolationTests(TenantIsolationCluster fixture) : IClassFixture<TenantIsolationCluster>
{
    private const string TenantId = "summit";
    private const string OtherTenantId = "other";

    private IGrainFactory GrainFactory => fixture.Cluster.GrainFactory;

    [Fact]
    public async Task AClientCall_ByAUserOfTheGrainsTenant_IsAllowed()
    {
        GivenSignedIn(User(TenantId));
        var key = TenantKey.Create(TenantId, "target");

        Assert.Equal(key, await GrainFactory.GetGrain<ITenantTargetGrain>(key).GetKeyAsync());
    }

    [Fact]
    public async Task AClientCall_ByAUserOfAnotherTenant_IsRejected()
    {
        GivenSignedIn(User(OtherTenantId));

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => GrainFactory.GetGrain<ITenantTargetGrain>(TenantKey.Create(TenantId, "target")).GetKeyAsync());

        Assert.Equal($"A user of tenant \"{OtherTenantId}\" attempted to access tenant \"{TenantId}\".", exception.Message);
    }

    [Fact]
    public async Task AClientCall_ByAUserWithoutATenant_IsRejected()
    {
        GivenSignedIn(User(tenantId: null));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => GrainFactory.GetGrain<ITenantTargetGrain>(TenantKey.Create(TenantId, "target")).GetKeyAsync());
    }

    [Fact]
    public async Task AClientCall_WithoutASignedInUser_IsRejected()
    {
        RequestContext.Clear();

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => GrainFactory.GetGrain<ITenantTargetGrain>(TenantKey.Create(TenantId, "target")).GetKeyAsync());

        Assert.Equal($"A signed-in user is required to access tenant \"{TenantId}\".", exception.Message);
    }

    [Fact]
    public async Task AClientCall_ToAGrainWithoutATenant_IsAllowedWithoutASignedInUser()
    {
        RequestContext.Clear();

        Assert.Equal("no-tenant", await GrainFactory.GetGrain<ITenantTargetGrain>("no-tenant").GetKeyAsync());
    }

    [Fact]
    public async Task AGrainCall_IntoTheGrainsOwnTenant_IsAllowed()
    {
        GivenSignedIn(User(TenantId));
        var caller = GrainFactory.GetGrain<ITenantCallerGrain>(TenantKey.Create(TenantId, "caller"));
        var targetKey = TenantKey.Create(TenantId, "target");

        Assert.Equal(targetKey, await caller.CallAsync(targetKey));
    }

    [Fact]
    public async Task AGrainCall_IntoAnotherTenant_IsRejected()
    {
        GivenSignedIn(User(TenantId));
        var caller = GrainFactory.GetGrain<ITenantCallerGrain>(TenantKey.Create(TenantId, "caller"));

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => caller.CallAsync(TenantKey.Create(OtherTenantId, "target")));

        Assert.Equal($"Tenant \"{TenantId}\" attempted to access tenant \"{OtherTenantId}\"", exception.Message);
    }

    /// <summary>A signed-in user of <paramref name="tenantId"/>, with the <c>tenant_id</c> claim GetRDone.Identity issues.</summary>
    private static ClaimsPrincipal User(string? tenantId)
    {
        List<Claim> claims = [new("sub", Guid.NewGuid().ToString())];
        if (tenantId is not null)
        {
            claims.Add(new Claim(ClientCallTenantFilter.TenantIdClaim, tenantId));
        }
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthentication"));
    }

    /// <summary>Signs in <paramref name="user"/> for the rest of the test; Orleans Identity reads the user from the request context.</summary>
    private static void GivenSignedIn(ClaimsPrincipal user) => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, user);
}
