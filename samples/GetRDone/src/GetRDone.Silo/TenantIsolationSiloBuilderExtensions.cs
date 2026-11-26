using Orleans.Multitenant;

namespace GetRDone.Silo;

public static class TenantIsolationSiloBuilderExtensions
{
    /// <summary>
    ///     Keeps every grain call inside one tenant: Orleans.Multitenant rejects grain calls into another tenant, and
    ///     <see cref="ClientCallTenantFilter"/> rejects calls from the web app and the worker into a tenant other than the
    ///     signed-in user's, which Orleans.Multitenant skips. No cross-tenant access is authorized.
    /// </summary>
    public static ISiloBuilder AddTenantIsolation(this ISiloBuilder silo)
    {
        silo.AddMultitenantCommunicationSeparation();
        silo.AddIncomingGrainCallFilter<ClientCallTenantFilter>();
        return silo;
    }
}
