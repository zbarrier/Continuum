# Get-R-Done sample

Get-R-Done (`GetRDone`) is a sample appraisal-management (AMC) application built on Continuum. Its domain is being added from the event model.

## Run

Requires Docker. From this folder:

```
dotnet run --project src/GetRDone.AppHost
```

Open the Aspire dashboard, then the `web` endpoint. Demo users (Development only, password `Pass123$`): `ana.ruiz@carterbank.example` (client user of Carter Bank & Trust), `coordinator@getrdone.local`, `reviewer@getrdone.local` (tenant `elliott`), and `vendor@getrdone.local` (no tenant). If your local Identity database was seeded before the tenant became `elliott`, delete the SQL Server data volume so the users are re-seeded. Vendors can also self-register from the sign-in page; confirmation and password-reset emails appear in Mailpit.

## Projects

- `GetRDone.AppHost` - Aspire: Azurite (Orleans clustering), KurrentDB, SQL Server (Identity, persistent volume), Mailpit, Identity, Silo, Web.
- `GetRDone.ServiceDefaults` - OpenTelemetry, health checks, service discovery.
- `GetRDone.Identity` - Duende IdentityServer with ASP.NET Core Identity on SQL Server and the Duende Razor Pages UI (`role`, `tenant_id` claims).
- `GetRDone.Silo` - Orleans silo; multi-tenant KurrentDB event log; ManagedCode.Orleans.Identity authorization.
- `GetRDone.Web` - Blazor Server; OIDC sign-in; forwards the circuit user to grains via an outgoing grain-call filter.
- `Features/<Domain>/GetRDone.<Domain>.Abstractions` / `.Grains` - feature slices. `Hello` is temporary and proves the round trip.

## Starting an order draft

The caller generates a new non-empty order GUID and calls `IOrderGrain` using that GUID as the tenant-qualified grain key (for example, `client.ForTenant(tenantId).GetGrain<IOrderGrain>(orderId.ToString())`). `StartOrderDraftAsync` returns the saved changes and stream version after persisting `OrderDraftStarted`. The event time is supplied by the backend, and `InitiatedBy` is the tenant-qualified user ID (`{tenantId}|{userGuid}`) built from the authenticated identity's GUID.

IDs (order, user, client, service request type, form type, intended use) are tenant-qualified strings validated by `TenantKeyValueObject` types; an ID from another tenant is rejected. The command carries the selected IDs, versions, and names, and only current versions are accepted.

The lookups come from pre-populated read models, seeded on activation when their state is empty (the event model's seed data chapter shows them being seeded by events this model does not produce). They are stored in memory per tenant, only the `elliott` tenant is seeded, and they are recreated after a silo restart. Use the `GetIntendedUsesGrain`, `GetOrderableServicesGrain`, and `GetUserDirectoryGrain` grain factory extensions to reach them:

- `IIntendedUsesGrain` (key `intendedUses`): Refinance, Purchase, Construction, Other. `IsCurrentIntendedUseAsync` checks the selected ID, version, and name.
- `IOrderableServicesGrain` (key `orderableServices`): one Appraisal Commercial service with its Market Value Interior Inspection form. `IsCurrentOrderableServiceAsync` checks the selected service and form IDs, versions, and names.
- `IUserDirectoryGrain` (key `userDirectory`): Ana Ruiz of Carter Bank & Trust. `IsClientUserAsync` checks that a user belongs to a client.

`IOrderPermissions` decides who may act on an order. Starting a draft requires the current user to be a user of the selected client; later commands will allow the user who started the order. Real tenant and permission checks belong to a full application. There is no order-entry UI yet.

Tenant-qualified keys partition the sample's data, but the current silo does not enforce that a caller's `tenant_id` claim matches the requested grain key. Authentication alone is not tenant authorization; do not rely on this sample for tenant isolation until that enforcement is added.

## Notes

- Uses Orleans 10.4.2 (required by ManagedCode.Orleans.Identity 10.4.6).
- `Directory.Build.props` does not import the repository root props, so the sample is not packaged.
