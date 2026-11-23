using GetRDone.Orders.Abstractions;

using ManagedCode.Orleans.Identity.Core.Constants;

namespace GetRDone.Orders.Automations;

public static class AutomationUserExtensions
{
    /// <summary>
    ///     Signs the account in for the grain calls that follow in the current async flow, as Orleans Identity reads the
    ///     user from the request context, which goes with each call to the silo. An automation that works on several
    ///     tenants signs in again for each one, inside its own async method so the change stays in that call.
    /// </summary>
    public static void SignIn(this AutomationUser account, string tenantId)
        => RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, account.CreatePrincipal(tenantId));
}
