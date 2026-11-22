# Get-R-Done sample

Get-R-Done (`GetRDone`) is a sample appraisal-management (AMC) application built on Continuum. Its domain is being added from the event model.

## Run

Requires Docker. From this folder:

```
dotnet run --project src/GetRDone.AppHost
```

Open the Aspire dashboard, then the `web` endpoint. Demo users (Development only, password `Pass123$`): `ana.ruiz@carterbank.example` (client user of Carter Bank & Trust), `coordinator@getrdone.local`, `reviewer@getrdone.local` (tenant `elliott`), and `vendor@getrdone.local` (no tenant). If your local Identity database was seeded before the tenant became `elliott`, delete the SQL Server data volume so the users are re-seeded. Vendors can also self-register from the sign-in page; confirmation and password-reset emails appear in Mailpit.

## Projects

- `GetRDone.AppHost` - Aspire: Azurite (Orleans clustering, and the `order-documents` blob container for supporting document files), KurrentDB, SQL Server (Identity, persistent volume), Mailpit, Identity, Silo, Web.
- `GetRDone.ServiceDefaults` - OpenTelemetry, health checks, service discovery.
- `GetRDone.Identity` - Duende IdentityServer with ASP.NET Core Identity on SQL Server and the Duende Razor Pages UI (`role`, `tenant_id` claims).
- `GetRDone.Silo` - Orleans silo; multi-tenant KurrentDB event log; KurrentDB stream provider for projections; ManagedCode.Orleans.Identity authorization.
- `GetRDone.Web` - Blazor Server; OIDC sign-in; forwards the circuit user to grains via an outgoing grain-call filter. A client user's Home has My drafts and My orders sections (paged lists). My drafts loads from the ClientOrderDrafts projection; My orders stays empty until the ClientOrders read model is built. Other roles' home pages are not built yet.
- `Features/<Domain>/GetRDone.<Domain>.Abstractions` / `.Grains` - feature slices. `Hello` is temporary and proves the round trip (the Hello page, `/hello`).

## Starting an order draft

The caller generates a new non-empty order GUID and calls `IOrderGrain` using that GUID as the tenant-qualified grain key (for example, `client.ForTenant(tenantId).GetGrain<IOrderGrain>(orderId.ToString())`). `StartOrderDraftAsync` returns the saved changes and stream version after persisting `OrderDraftStarted`. The event time is supplied by the backend, and `InitiatedBy` is the tenant-qualified user ID (`{tenantId}|{userGuid}`) built from the authenticated identity's GUID.

IDs (order, user, client, service request type, form type, intended use) are tenant-qualified strings validated by `TenantKeyValueObject` types; an ID from another tenant is rejected. The command carries the selected IDs, versions, and names, and only current versions are accepted.

The lookups come from pre-populated read models, seeded on activation when their state is empty (the event model's seed data chapter shows them being seeded by events this model does not produce). The seed data comes from the registered `IReadModelSeedData`: the silo registers `SampleSeedData`, and the tests register their own data. The read models are stored in memory per tenant, only the `elliott` tenant is seeded, and they are recreated after a silo restart. Each tenant has one instance of each read model grain, keyed `{tenantId}|default`; use the `GetIntendedUsesGrain`, `GetDocumentTypesGrain`, `GetOrderableServicesGrain`, and `GetUserDirectoryGrain` grain factory extensions to reach them with just the tenant ID:

- `IIntendedUsesGrain` (grain type `intendedUses`): Refinance, Purchase, Construction, Other. `IsCurrentIntendedUseAsync` checks the selected ID, version, and name.
- `IDocumentTypesGrain` (grain type `documentTypes`): the types a supporting document can have: Lease agreements, Rent roll, Operating statements, Other. `IsCurrentDocumentTypeAsync` checks the selected ID, version, and name.
- `IOrderableServicesGrain` (grain type `orderableServices`): the service request types, each with the form types that can be ordered with it (the user picks a service request type, then one of its form types). Appraisal Commercial has Market Value Interior Inspection and Market Value Exterior Inspection; Appraisal Residential has Uniform Residential Appraisal Report (1004) and Exterior-Only Inspection (2055). `IsCurrentOrderableServiceAsync` checks the selected service and form IDs, versions, and names, and that the form belongs to the service.
- `IUserDirectoryGrain` (grain type `userDirectory`): Ana Ruiz of Carter Bank & Trust. `IsClientUserAsync` checks that a user belongs to a client.

`IOrderPermissions` decides who may act on an order. Starting a draft requires the current user to be a user of the selected client; later commands will allow the user who started the order. Real tenant and permission checks belong to a full application. There is no order-entry UI yet.

## Supporting documents

Order entry tab 3 attaches supporting documents to a draft. Files are stored in blob storage (Azurite locally), never in events: the web app generates the document ID (`{tenantId}|{guid}`), uploads the file to `SupportingDocumentStorage.BlobPath` (`{tenantId}/{orderGuid}/{documentGuid}` in the `order-documents` container), and then calls `AttachSupportingDocumentAsync` with the file's name, content type, and size. `SupportingDocumentAttached` records the blob path. Files can be up to 100 MB, of any type. The upload never overwrites an existing blob. When the order rejects the document (a failed `Result`), the web app deletes the upload as a best effort; when the call throws, it keeps the file, since the order may have attached it. Removing a document and downloading its file come with later slices.

## Client order projections

Every order event also targets the per-user stream `clientUserOrders-{tenantId}|{userGuid}` of the user who started the order (`OrderGrain.GetStreamTargets`), so one projection can group all of a user's orders. The Silo's `OrderProjections` KurrentDB stream provider runs a catch-up subscription on `$all` filtered to the `order-` stream prefix and delivers each event to its order stream and its target streams. Projection grains implicitly subscribe to the `OrderProjections:clientUserOrders` namespace, so Orleans activates one grain per user, keyed by the user's tenant user ID.

- `IClientOrderDraftsGrain` (grain type `clientOrderDrafts`; `GetClientOrderDraftsGrain(tenantUserId)`): the user's drafts, most recently saved first. `GetDraftsAsync(skip, take)` returns one page and the total count. Only the user can read their own drafts.

The checkpoint and the projections are stored in memory, so after a silo restart the subscription replays from the start and rebuilds them.

Tenant-qualified keys partition the sample's data, but the current silo does not enforce that a caller's `tenant_id` claim matches the requested grain key. Authentication alone is not tenant authorization; do not rely on this sample for tenant isolation until that enforcement is added.

## Notes

- Uses Orleans 10.4.2 (required by ManagedCode.Orleans.Identity 10.4.6).
- `Directory.Build.props` does not import the repository root props, so the sample is not packaged.
