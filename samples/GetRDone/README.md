# Get-R-Done sample

Get-R-Done (`GetRDone`) is a sample appraisal-management (AMC) application built on Continuum. Its domain is being added from the event model.

## Run

Requires Docker. From this folder:

```
dotnet run --project src/GetRDone.AppHost
```

Open the Aspire dashboard, then the `web` endpoint. Demo users (Development only, password `Pass123$`): `ana.ruiz@carterbank.example` (client user of Carter Bank & Trust), `scott.riggsbee@elliott.example` (Elliott & Company's Client Services Director, a coordinator), `coordinator@getrdone.local`, `reviewer@getrdone.local` (tenant `elliott`), and `vendor@getrdone.local` (no tenant). The automations' service accounts (`order-number-assigner@` and `new-order-notifier@elliott.example`) are seeded with no password, so nobody can sign in as them. If your local Identity database was seeded before the tenant became `elliott`, delete the SQL Server data volume so the users are re-seeded. Vendors can also self-register from the sign-in page; confirmation and password-reset emails appear in Mailpit.

## Projects

- `GetRDone.AppHost` - Aspire: Azurite (Orleans clustering, and the `order-documents` blob container for supporting document files), KurrentDB, SQL Server (Identity, persistent volume), Mailpit, Identity, Silo, Subscriptions Worker, Web.
- `GetRDone.ServiceDefaults` - OpenTelemetry, health checks, service discovery.
- `GetRDone.Identity` - Duende IdentityServer with ASP.NET Core Identity on SQL Server and the Duende Razor Pages UI (`role`, `tenant_id` claims).
- `GetRDone.Silo` - Orleans silo; multi-tenant KurrentDB event log; KurrentDB stream provider for projections; ManagedCode.Orleans.Identity authorization.
- `GetRDone.Web` - Blazor Server; OIDC sign-in; forwards the circuit user to grains via an outgoing grain-call filter. A client user's Home has My drafts and My orders sections (paged lists). My drafts loads from the ClientOrderDrafts projection, and My orders from the ClientOrders projection. Other roles' home pages are not built yet.
- `GetRDone.Subscriptions.Worker` - hosts the self-hosted KurrentDB catch-up subscriptions of automations that work in batches (one folder per feature) and calls the silo's grains as an Orleans cluster client. It runs as exactly one instance (`WithReplicas(1)`), since a catch-up subscription doesn't share its work with other instances. Its health check reports the subscriptions' state. `appsettings.json` lists every subscription under `Continuum:KurrentDB:Subscriptions:{Name}`; set `Enabled` to `false` to stop one without a code change (it keeps its checkpoint and catches up when turned back on).
- `Features/<Domain>/GetRDone.<Domain>.Abstractions` / `.Grains` / `.Automations` - feature slices. `.Automations` holds the logic of the domain's automations, which the worker runs; like any client it references only `.Abstractions`. `Hello` is temporary and proves the round trip (the Hello page, `/hello`).

## Starting an order draft

The caller generates a new non-empty order GUID and calls `IOrderGrain` using that GUID as the tenant-qualified grain key (for example, `client.ForTenant(tenantId).GetGrain<IOrderGrain>(orderId.ToString())`). `StartOrderDraftAsync` returns the saved changes and stream version after persisting `OrderDraftStarted`. The event time is supplied by the backend, and `InitiatedBy` is the tenant-qualified user ID (`{tenantId}|{userGuid}`) built from the authenticated identity's GUID.

IDs (order, user, client, service request type, form type, intended use) are tenant-qualified strings validated by `TenantKeyValueObject` types; an ID from another tenant is rejected. The command carries the selected IDs, versions, and names, and only current versions are accepted.

The lookups come from pre-populated read models, seeded on activation when their state is empty (the event model's seed data chapter shows them being seeded by events this model does not produce). The seed data comes from the registered `IReadModelSeedData`: the silo registers `SampleSeedData`, and the tests register their own data. The read models are stored in memory per tenant, only the `elliott` tenant is seeded, and they are recreated after a silo restart. Each tenant has one instance of each read model grain, keyed `{tenantId}|default`; use the `GetIntendedUsesGrain`, `GetDocumentTypesGrain`, `GetOrderableServicesGrain`, and `GetUserDirectoryGrain` grain factory extensions to reach them with just the tenant ID:

- `IIntendedUsesGrain` (grain type `intendedUses`): Refinance, Purchase, Construction, Other. `IsCurrentIntendedUseAsync` checks the selected ID, version, and name.
- `IDocumentTypesGrain` (grain type `documentTypes`): the types a supporting document can have: Lease agreements, Rent roll, Operating statements, Other. `IsCurrentDocumentTypeAsync` checks the selected ID, version, and name.
- `IOrderableServicesGrain` (grain type `orderableServices`): the service request types, each with the form types that can be ordered with it (the user picks a service request type, then one of its form types). Appraisal Commercial has Market Value Interior Inspection and Market Value Exterior Inspection; Appraisal Residential has Uniform Residential Appraisal Report (1004) and Exterior-Only Inspection (2055). `IsCurrentOrderableServiceAsync` checks the selected service and form IDs, versions, and names, and that the form belongs to the service.
- `IUserDirectoryGrain` (grain type `userDirectory`): Ana Ruiz of Carter Bank & Trust, Scott Riggsbee of Elliott & Company, and the automations' service accounts. `IsClientUserAsync` checks that a user belongs to a client.

`IOrderPermissions` decides who may act on an order. Starting a draft requires the current user to be a user of the selected client; later commands will allow the user who started the order. Real tenant and permission checks belong to a full application. There is no order-entry UI yet.

## Supporting documents

Order entry tab 3 attaches supporting documents to a draft. Files are stored in blob storage (Azurite locally), never in events: the web app generates the document ID (`{tenantId}|{guid}`), uploads the file to `SupportingDocumentStorage.BlobPath` (`{tenantId}/{orderGuid}/{documentGuid}` in the `order-documents` container), and then calls `AttachSupportingDocumentAsync` with the file's name, content type, and size. `SupportingDocumentAttached` records the blob path. Files can be up to 100 MB, of any type. The upload never overwrites an existing blob. When the order rejects the document (a failed `Result`), the web app deletes the upload as a best effort; when the call throws, it keeps the file, since the order may have attached it. Removing a document (the delete button on tab 3, after a confirmation) calls `RemoveSupportingDocumentAsync`; `SupportingDocumentRemoved` records the blob path the document was attached with, and the web app deletes that file as a best effort once the removal succeeded. A failed or throwing removal keeps the file. Downloading a document's file comes with a later slice.

## Submitting an order

Order entry tab 4 (Review & Submit) shows everything saved on the draft and calls `SubmitOrderAsync`, which records `OrderSubmitted` with the draft's client. The draft needs a borrower (tab 2); supporting documents are optional, and the tab opens only once the borrower is saved. A submitted order can't be submitted again or changed with the draft commands (`FailedPrecondition`), and `GetOrderDraftAsync` rejects it, since it is no longer a draft. After submitting, the web app goes back to Home, where the draft has moved from My drafts to My orders. The order shows its number as Pending until it is assigned one (see Order numbers).

## Order numbers

Each tenant numbers its submitted orders one after another, starting at 610696. Two automations do this, both running as the `OrderNumberAssigner` service account (`AutomationUser`) on self-hosted KurrentDB catch-up subscriptions in `GetRDone.Subscriptions.Worker` (`Orders`), which read `$all` in batches and call the silo as a cluster client. Their logic is in `GetRDone.Orders.Automations`:

1. **Order Number Issuer** (`OrderNumberIssuerSubscription`, prefix `order-`): for the `OrderSubmitted` events of a batch, asks each tenant's `IOrderNumberGeneratorGrain` (grain type `orderNumberGenerator`, keyed `{tenantId}|default`; `GetOrderNumberGeneratorGrain(tenantId)`) to issue numbers with `IssueOrderNumbersAsync`, which records `OrderNumbersIssued`. The generator issues an order only one number: asking again returns the number already issued and records nothing.
2. **Order Number Assigner** (`OrderNumberAssignerSubscription`, prefix `orderNumberGenerator-`): for the `OrderNumbersIssued` events of a batch, calls `AssignOrderNumberAsync` on each order in parallel, which records `OrderNumberAssigned` with the order's client. An order that already has its number succeeds without changes.

Only the `OrderNumberAssigner` account may issue or assign numbers (`IOrderPermissions`). Numbers are required, so an order is skipped only when it can never take a number: its ID is invalid, or it doesn't exist or isn't submitted (gRPC `InvalidArgument`, `OutOfRange`, `NotFound`, `FailedPrecondition` or `AlreadyExists`). Numbers are never reserved, so a number issued to such an order leaves a gap. When one order of a tenant's batch can never be issued a number, the issuer issues the others one at a time so only that order is skipped. Every other failure (an exception, a storage or silo outage, a bug, a missing permission) is retried with the same numbers, and the subscription doesn't move its checkpoint past the batch until no order is waiting; the assigner retries only the orders still waiting. The ClientOrders projection shows the number once `OrderNumberAssigned` arrives. Both subscriptions keep their checkpoints in KurrentDB (`AddKurrentDBCheckpointStore`), since what they produce is stored there too.

## Client order projections

Every order event also targets the per-user stream `clientUserOrders-{tenantId}|{userGuid}` of the user who started the order (`OrderGrain.GetStreamTargets`), so one projection can group all of a user's orders. The Silo's `OrderProjections` KurrentDB stream provider runs a catch-up subscription on `$all` filtered to the `order-` stream prefix and delivers each event to its order stream and its target streams. Projection grains implicitly subscribe to the `OrderProjections:clientUserOrders` namespace, so Orleans activates one grain per user, keyed by the user's tenant user ID.

- `IClientOrderDraftsGrain` (grain type `clientOrderDrafts`; `GetClientOrderDraftsGrain(tenantUserId)`): the user's drafts that are not submitted yet, most recently saved first (`OrderSubmitted` removes a draft). `GetDraftsAsync(skip, take)` returns one page and the total count. Only the user can read their own drafts.
- `IClientOrdersGrain` (grain type `clientOrders`; `GetClientOrdersGrain(tenantUserId)`): the user's submitted orders, most recently submitted first, each with its property address, form type, order number (null until one is assigned) and status (`ClientOrderStatus`, only `Submitted` so far). It keeps the address and form type of the user's drafts from `OrderDraftStarted` and `RequestAndPropertySaved`, since `OrderSubmitted` carries neither, and `OrderSubmitted` turns the draft into an order. `GetOrdersAsync(skip, take)` returns one page and the total count. Only the user can read their own orders.

The checkpoint and the projections are stored in memory, so after a silo restart the subscription replays from the start and rebuilds them.

Tenant-qualified keys partition the sample's data, but the current silo does not enforce that a caller's `tenant_id` claim matches the requested grain key. Authentication alone is not tenant authorization; do not rely on this sample for tenant isolation until that enforcement is added.

## Notes

- Uses Orleans 10.4.2 (required by ManagedCode.Orleans.Identity 10.4.6).
- `Directory.Build.props` does not import the repository root props, so the sample is not packaged.
