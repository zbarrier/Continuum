using System.Security.Claims;

using Continuum.Orleans.Tenancy;

using ManagedCode.Orleans.Identity.Core.Constants;

namespace GetRDone.Silo;

/// <summary>
///     Keeps cluster clients, such as GetRDone.Web and GetRDone.Subscriptions.Worker, inside the signed-in user's tenant:
///     a client call to a tenant grain, keyed <c>{TenantId}|{Key}</c>, must carry a signed-in user whose
///     <c>tenant_id</c> claim is that tenant, or it fails with <see cref="UnauthorizedAccessException"/>.
/// </summary>
/// <remarks>
///     Orleans.Multitenant's communication separation only checks calls from grains, so this filter covers the calls it
///     skips. Calls from grains and system targets are left to it, and calls to grains without a tenant and to
///     <c>Orleans.*</c> interfaces are let through, as it does.
/// </remarks>
internal sealed class ClientCallTenantFilter : IIncomingGrainCallFilter
{
    /// <summary>The claim holding the tenant a user belongs to, as GetRDone.Identity issues it.</summary>
    public const string TenantIdClaim = "tenant_id";

    public Task Invoke(IIncomingGrainCallContext context)
    {
        if (IsFromClient(context) && !context.InterfaceName.StartsWith("Orleans.", StringComparison.Ordinal))
        {
            ThrowIfOutsideTheUsersTenant(context.TargetId);
        }
        return context.Invoke();
    }

    private static bool IsFromClient(IIncomingGrainCallContext context)
    {
        if (context.SourceId is not { } sourceId)
        {
            return true;
        }
        var sourceType = sourceId.Type;
        return sourceType.IsClient();
    }

    private static void ThrowIfOutsideTheUsersTenant(GrainId targetId)
    {
        if (!TenantKey.TrySplit(targetId.Key.ToString(), out var targetTenantId, out _))
        {
            throw new UnauthorizedAccessException($"The key of {targetId} is not a valid tenant key.");
        }
        if (targetTenantId is null)
        {
            return;
        }

        var user = RequestContext.Get(OrleansIdentityConstants.USER_CLAIMS) as ClaimsPrincipal;
        if (user?.Identity is not { IsAuthenticated: true })
        {
            throw new UnauthorizedAccessException($"A signed-in user is required to access tenant \"{targetTenantId}\".");
        }

        var userTenantId = user.FindFirstValue(TenantIdClaim);
        if (!string.Equals(userTenantId, targetTenantId, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException(
                $"A user of tenant \"{userTenantId ?? "NULL"}\" attempted to access tenant \"{targetTenantId}\".");
        }
    }
}
