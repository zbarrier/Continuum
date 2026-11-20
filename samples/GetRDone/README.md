# Get-R-Done sample

Get-R-Done (`GetRDone`) is a sample appraisal-management (AMC) application built on Continuum. Its domain is being added from the event model.

## Run

Requires Docker. From this folder:

```
dotnet run --project src/GetRDone.AppHost
```

Open the Aspire dashboard, then the `web` endpoint. Demo users (Development only, password `Pass123$`): `client@getrdone.local`, `coordinator@getrdone.local`, `reviewer@getrdone.local` (tenant `demo`), and `vendor@getrdone.local` (no tenant). Vendors can also self-register from the sign-in page; confirmation and password-reset emails appear in Mailpit.

## Projects

- `GetRDone.AppHost` - Aspire: Azurite (Orleans clustering), KurrentDB, SQL Server (Identity, persistent volume), Mailpit, Identity, Silo, Web.
- `GetRDone.ServiceDefaults` - OpenTelemetry, health checks, service discovery.
- `GetRDone.Identity` - Duende IdentityServer with ASP.NET Core Identity on SQL Server and the Duende Razor Pages UI (`role`, `tenant_id` claims).
- `GetRDone.Silo` - Orleans silo; multi-tenant KurrentDB event log; ManagedCode.Orleans.Identity authorization.
- `GetRDone.Web` - Blazor Server; OIDC sign-in; forwards the circuit user to grains via an outgoing grain-call filter.
- `Features/<Domain>/GetRDone.<Domain>.Abstractions` / `.Grains` - feature slices. `Hello` is temporary and proves the round trip.

## Starting an order draft

The caller generates a new non-empty order GUID and calls `IOrderGrain` using that GUID as the tenant-qualified grain key (for example, `client.ForTenant(tenantId).GetGrain<IOrderGrain>(orderId.ToString())`). `StartOrderDraftAsync` returns the order ID after persisting `OrderDraftStarted`. The authenticated user's ID and event time are supplied by the backend.

`IOrderableServicesGrain` uses the tenant-qualified key `offerings`. It exposes a hard-coded sample service/form pair, stored in memory per tenant and recreated after a silo restart. The command checks the selected IDs and versions against that list; client accounts and divisions from the original model are not part of this sample. There is no order-entry UI yet.

Tenant-qualified keys partition the sample's data, but the current silo does not enforce that a caller's `tenant_id` claim matches the requested grain key. Authentication alone is not tenant authorization; do not rely on this sample for tenant isolation until that enforcement is added.

## Notes

- Uses Orleans 10.4.2 (required by ManagedCode.Orleans.Identity 10.4.6).
- `Directory.Build.props` does not import the repository root props, so the sample is not packaged.
